using System.Diagnostics;
using BlacksmithCore.AI;
using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Components;

namespace BlacksmithClient.Frontend
{
    public class WebGameSession
    {
        private readonly List<IAIStrategy> _strategies;
        private readonly List<double> _thinkingTimesMs = new();
        private GameInstance? _game;
        private IAIStrategy? _activeAI;
        private int _mode;
        private bool _started;
        private string _modeName = string.Empty;
        private bool _isManual;
        private Task<(string skillName, int param, string stringParam)>? _pendingAI;
        private CancellationTokenSource? _cts;

        public WebGameSession(List<IAIStrategy> strategies)
        {
            _strategies = strategies;
        }

        public List<object> GetStrategies()
        {
            var list = new List<object>();
            for (int i = 0; i < _strategies.Count; i++)
            {
                list.Add(new { id = i, name = _strategies[i].Name + " (AI)" });
            }
            list.Add(new { id = _strategies.Count, name = "Manual" });
            return list;
        }

        public object StartGame(int mode)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _pendingAI = null;

            var starter = new BackendStarter();
            _game = starter.StartBackend();
            _mode = mode;
            _started = true;
            _thinkingTimesMs.Clear();

            _isManual = mode >= _strategies.Count;
            if (_isManual)
            {
                _activeAI = null;
                _modeName = "Manual";
            }
            else
            {
                _activeAI = _strategies[mode];
                _activeAI.Init(_game);
                _modeName = _activeAI.Name;
            }

            StartAIComputation();

            return BuildSnapshot();
        }

        private void StartAIComputation()
        {
            if (_isManual || _activeAI == null || _game == null)
                return;

            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            _pendingAI = Task.Run(() => _activeAI.ChooseSkill(), ct);
        }

        public async Task<DeclareResult> DeclareTurnAsync(string skillName, int param, string esn, int ep, string stringParam = "", string esp = "")
        {
            if (_game == null || !_started)
                return new DeclareResult { Ok = false, Message = "Game not started.", Snapshot = GetSnapshot() };

            var playerResult = _game.TryDeclare(skillName, param, stringParam);
            if (playerResult != SkillDeclareResult.Success)
                return new DeclareResult { Ok = false, Message = $"Player skill '{skillName}' {playerResult}.", Snapshot = GetSnapshot() };

            string enemySkillName;
            int enemyParam;
            string enemyStringParam;

            if (!_isManual && _activeAI != null)
            {
                var sw = Stopwatch.StartNew();
                if (_pendingAI == null)
                {
                    (enemySkillName, enemyParam, enemyStringParam) = _activeAI.ChooseSkill();
                }
                else
                {
                    (enemySkillName, enemyParam, enemyStringParam) = await _pendingAI;
                    _pendingAI = null;
                }
                sw.Stop();
                _thinkingTimesMs.Add(sw.Elapsed.TotalMilliseconds);
            }
            else
            {
                _thinkingTimesMs.Add(0);
                var enemyResult = _game.ETryDeclare(esn, ep, esp);
                if (enemyResult != SkillDeclareResult.Success)
                    return new DeclareResult { Ok = false, Message = $"Enemy skill '{esn}' {enemyResult}.", Snapshot = GetSnapshot() };

                enemySkillName = esn;
                enemyParam = ep;
                enemyStringParam = esp;
            }

            _game.Declare(skillName, param, enemySkillName, enemyParam, stringParam, enemyStringParam);

            StartAIComputation();

            var snapshot = BuildSnapshot();
            return new DeclareResult { Ok = true, Message = "Turn resolved.", Snapshot = snapshot };
        }

        public object GetSnapshot()
        {
            return BuildSnapshot();
        }

        private object BuildSnapshot()
        {
            if (_game == null || !_started)
            {
                return new
                {
                    player = (object?)null,
                    enemy = (object?)null,
                    turns = Array.Empty<object>(),
                    started = false,
                    manualMode = true,
                    modeName = "Not started",
                    result = "Awaiting game"
                };
            }

            var pv = _game.Player.Focus.GetView();
            var ev = _game.Enemy.Focus.GetView();

            return new
            {
                player = BuildActor(pv, _game.Player.Focus.Get<Skill>().GetAvailableSkillNames(), _game.Player),
                enemy = BuildActor(ev, _game.Enemy.Focus.Get<Skill>().GetAvailableSkillNames(), _game.Enemy),
                turns = _game.History.SkillHistory.Select((pair, i) => new
                {
                    index = i + 1,
                    result = "Continue",
                    playerSkill = pair.Item1.SkillName,
                    playerParam = pair.Item1.Param,
                    playerStringParam = pair.Item1.StringParam,
                    enemySkill = pair.Item2.SkillName,
                    enemyParam = pair.Item2.Param,
                    enemyStringParam = pair.Item2.StringParam,
                    thinkingTimeMs = i < _thinkingTimesMs.Count ? _thinkingTimesMs[i] : 0.0
                }).ToList(),
                started = _started,
                manualMode = _isManual,
                modeName = _modeName,
                result = DetermineResult(pv, ev)
            };
        }

        private static object BuildActor(BodyView view, List<string> availableSkills, Community? community = null)
        {
            var summons = new List<object>();
            if (community != null)
            {
                summons = community.SummonList.Select(s =>
                {
                    var sv = s.GetView();
                    return BuildActor(sv, s.Get<Skill>().GetAvailableSkillNames(), null);
                }).ToList();
            }

            return new
            {
                bodyName = view.BodyName,
                professions = view.ProfessionNames,
                hp = view.HP,
                maxHP = view.MHP,
                defenses = view.DefenseView.Select(d => new { name = d.name, power = d.power }).ToList(),
                resources = view.ResourcesView.Select(r => new { name = r.name, quantity = r.quantity }).ToList(),
                futureAttacks = view.FutureAttackView.Select(f => new
                {
                    name = f.name,
                    delayRounds = f.delayRounds,
                    power = f.power
                }).ToList(),
                futureDefenses = view.FutureDefenseView.Select(f => new
                {
                    name = f.name,
                    delayRounds = f.delayRounds,
                    power = f.power
                }).ToList(),
                availableSkills = availableSkills,
                summons = summons
            };
        }

        private static string DetermineResult(BodyView player, BodyView enemy)
        {
            bool playerDead = player.HP <= 0;
            bool enemyDead = enemy.HP <= 0;

            if (playerDead && enemyDead) return "draw";
            if (playerDead) return "lose";
            if (enemyDead) return "win";
            return "next";
        }
    }

    public class DeclareResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Snapshot { get; set; }
    }
}
