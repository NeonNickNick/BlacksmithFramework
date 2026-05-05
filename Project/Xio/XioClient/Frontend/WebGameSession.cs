using ClapInfra.ClapModels.Components;
using XioCore.Driver;
using XioCore.Infra.Models.Components;
using XioCore.Infra.Models.Entities;

namespace XioClient.Frontend
{
    public class WebGameSession
    {
        private static readonly Dictionary<string, int> RankPrefixes = new()
        {
            { "hj", 1 },
            { "zs", 2 },
            { "hd", 3 },
            { "wx", 4 }
        };

        private static readonly Dictionary<int, string> RankNames = new()
        {
            { 0, "Default" },
            { 1, "Golden" },
            { 2, "Diamond" },
            { 3, "BlackHole" },
            { 4, "WanXiang" }
        };

        private GameInstance? _game;
        private int _mode;
        private bool _started;
        private string _modeName = string.Empty;

        public List<object> GetStrategies()
        {
            return new List<object>
            {
                new { id = 0, name = "Manual" }
            };
        }

        public object StartGame(int mode)
        {
            var starter = new BackendStarter();
            _game = starter.StartBackend();
            _mode = mode;
            _started = true;
            _modeName = "Manual";
            return BuildSnapshot();
        }

        public DeclareResult DeclareTurn(string skillInput, string enemySkillInput)
        {
            if (_game == null || !_started)
                return new DeclareResult { Ok = false, Message = "Game not started.", Snapshot = GetSnapshot() };

            var (skillName, rank) = ParseSkillInput(skillInput);
            var (esn, er) = ParseSkillInput(enemySkillInput);

            var playerResult = _game.TryDeclare(skillName, rank);
            if (playerResult != SkillDeclareResult.Success)
                return new DeclareResult { Ok = false, Message = $"Player skill '{skillName}' {playerResult}.", Snapshot = GetSnapshot() };

            var enemyResult = _game.ETryDeclare(esn, er);
            if (enemyResult != SkillDeclareResult.Success)
                return new DeclareResult { Ok = false, Message = $"Enemy skill '{esn}' {enemyResult}.", Snapshot = GetSnapshot() };

            _game.Declare(skillName, rank, esn, er);

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
                    round = 0,
                    innerRound = 0,
                    player = (object?)null,
                    enemy = (object?)null,
                    turns = Array.Empty<object>(),
                    started = false,
                    manualMode = true,
                    modeName = "Not started",
                    result = "Awaiting game"
                };
            }

            var pv = _game.Player.GetView();
            var ev = _game.Enemy.GetView();

            return new
            {
                round = pv.Round,
                innerRound = pv.InnerRound,
                player = BuildActor(pv, _game.Player),
                enemy = BuildActor(ev, _game.Enemy),
                turns = _game.History.SkillHistory.Select((pair, i) => new
                {
                    index = i + 1,
                    result = "Continue",
                    playerSkill = pair.Item1.SkillName,
                    playerRank = pair.Item1.Rank,
                    enemySkill = pair.Item2.SkillName,
                    enemyRank = pair.Item2.Rank
                }).ToList(),
                started = _started,
                manualMode = true,
                modeName = _modeName,
                result = "next"
            };
        }

        private static object BuildActor(BodyView view, Body body)
        {
            var skill = body.Get<Skill>();
            var skillInfos = skill.GetAvailableSkillNames().Select(name =>
            {
                var context = new DefaultSkillContext(view.Rank, name, body);
                var result = skill.TryDeclare(name, context);
                return new { name, usable = result == SkillDeclareResult.Success };
            }).ToList();

            return new
            {
                rank = view.Rank,
                rankName = RankNames.GetValueOrDefault(view.Rank, "Unknown"),
                innerLevel = view.InnerLevel,
                resources = view.ResourceView.Select(r => new { name = r.name, quantity = r.quantity }).ToList(),
                availableSkills = skillInfos
            };
        }

        private static (string skillName, int rank) ParseSkillInput(string input)
        {
            var trimmed = (input ?? string.Empty).Trim();
            if (trimmed.Length == 0)
                return ("x", 0);

            foreach (var (prefix, rank) in RankPrefixes)
            {
                var prefixWithDash = prefix + "-";
                if (trimmed.StartsWith(prefixWithDash, StringComparison.OrdinalIgnoreCase))
                {
                    var skillName = trimmed.Substring(prefixWithDash.Length);
                    return (skillName.Length > 0 ? skillName : "x", rank);
                }
            }

            return (trimmed, 0);
        }
    }

    public class DeclareResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Snapshot { get; set; }
    }
}
