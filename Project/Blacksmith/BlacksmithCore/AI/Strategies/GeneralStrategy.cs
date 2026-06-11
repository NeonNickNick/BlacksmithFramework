using System.Text.Json.Serialization;
using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Components;


namespace BlacksmithCore.AI.Strategies
{
    public class GeneralStrategyParams
    {
        [JsonConstructor]
        public GeneralStrategyParams()
        {

        }
        public double TemperatureCoefficient = 0.03; // 原 0.03 * round
        // ========== 早期资源权重 ==========
        public double EarlyIronWeight = 120;
        public double EarlyExcessIronWeight = 102;     // 超过4铁时的额外奖励
        public double EarlySpaceWeight = 4;
        public double EarlyTimeWeight = 3.5;
        public double EarlyDefaultWeight = 3;

        // ========== 中期资源权重 ==========
        public double MidIronWeight = 12;
        public double MidSpaceWeight = 10;
        public double MidTimeWeight = 9;
        public double MidDefaultWeight = 6;

        // ========== 后期资源权重 ==========
        public double LateIronWeight = 2;
        public double LateSpaceWeight = 2.5;
        public double LateTimeWeight = 2.5;
        public double LateDefaultWeight = 1.5;


        // ========== 攻击策略权重 ==========
        public double EarlyUnnecessaryAttackPenaltyMultiplier = 40;    // (10 - HP) * 30
        public double MidAdvantageAttackBonusMultiplier = -10;           // hpDiff * 2
        public double MidUnnecessaryAttackPenaltyMultiplier = 0;      // (10 - HP) * 10
        public double WithProfessionDamageBonusMultiplier = 0;        // (10 - HP) * 20
        public double WithProfessionHpDiffBonusMultiplier = 0;         // hpDiff * 5
        public double HpAdvantageThreshold = 0;                       // hpDiff > 2 才算优势

        // ========== 回合节奏 ==========
        public double LateRoundPenaltyPerRound = 40;

        // ========== 终局分数 ==========
        public double WinScore = 100;
    }
    public class GeneralStrategy : IAIStrategy
    {
        private readonly GeneralStrategyParams _params;
        private GameInstance _main = null!;
        private static readonly HashSet<string> _uselessActions = new()
        {
            //"stick", "drill", "recovery", "shield", "thornshield", "mute"
        };
        private static ThreadLocal<Random> _random = new(() =>
        {
            return new Random(Random.Shared.Next());
        });

        public string Name => "General";

        public GeneralStrategy(GeneralStrategyParams? parameters = null)
        {
            _params = parameters ?? new GeneralStrategyParams();
        }

        public void Init(GameInstance gameInstance)
        {
            _main = gameInstance;
        }

        public (string skillName, int param, string stringParam) ChooseSkill()
        {
            int threadCount = Math.Min(7, Environment.ProcessorCount);
            var tasks = new List<Task<List<MCTSNode>>>();

            for (int t = 0; t < threadCount; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var localGame = _main.DeepCopy();
                    return RunMCTS(localGame, 7000 / threadCount);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // 合并 root children
            var merged = new Dictionary<(string, int, string), (double wins, int visits)>();
            foreach (var task in tasks)
            {
                foreach (var child in task.Result)
                {
                    var action = child.Action!.Value;
                    if (!merged.ContainsKey(action))
                        merged[action] = (0, 0);

                    var v = merged[action];
                    v.wins += child.Wins;
                    v.visits += child.Visits;
                    merged[action] = v;
                }
            }

            var finalChildren = merged.Select(kv => new MCTSNode(null!, null!, new List<(string, int, string)>())
            {
                Action = kv.Key,
                Wins = kv.Value.wins,
                Visits = kv.Value.visits
            }).ToList();

            return SampleFromTopK(finalChildren, _main.History.SkillHistory.Count);
        }

        private List<MCTSNode> RunMCTS(GameInstance rootState, int iterations)
        {
            var rootActions = GetAllAvailable(rootState.Enemy, rootState);
            var root = new MCTSNode(rootState, null, rootActions);

            for (int i = 0; i < iterations; i++)
            {
                var node = root;

                // Selection
                while (node.UntriedActions.Count == 0 && node.Children.Count > 0)
                {
                    node = Select(node);
                }

                if (node.UntriedActions.Count > 0)
                {
                    EnsureState(node);

                    var action = node.UntriedActions[0];
                    node.UntriedActions.RemoveAt(0);

                    // 一次 DeepCopy 同时用于 expansion 与 rollout
                    var simState = node.State!.DeepCopy();
                    var playerAction = RandomAction(simState.Player, simState);
                    simState.Declare(
                        playerAction.Item1, playerAction.Item2,
                        action.Item1, action.Item2,
                        playerAction.Item3, action.Item3
                    );

                    // 在 rollout 覆盖之前捕获子节点的合法动作
                    var nextActions = GetAllAvailable(simState.Enemy, simState);

                    // 创建延迟子节点
                    var child = new MCTSNode(null, node, nextActions)
                    {
                        Action = action,
                        PlayerAction = playerAction
                    };
                    node.Children.Add(child);
                    node = child;


                    double result = Evaluate(simState);
                    // Rollout 从已应用 expansion 的同一状态开始
                    for (int d = 1; d < 2; d++)
                    {
                        if (IsTerminal(simState))
                            break;

                        var p = RandomAction(simState.Player, simState);
                        var e = RandomAction(simState.Enemy, simState);
                        simState.Declare(p.Item1, p.Item2, e.Item1, e.Item2, p.Item3, e.Item3);

                        result = (result * d + MathF.Exp(-0.3f * d) * Evaluate(simState));
                        result /= d;
                    }


                    // Backprop
                    while (node != null)
                    {
                        node.Visits++;
                        node.Wins += result;
                        node = node.Parent!;
                    }

                    simState.ReturnToPool();
                }
                else
                {
                    // 终端节点
                    EnsureState(node);
                    double result = Evaluate(node.State!);
                    while (node != null)
                    {
                        node.Visits++;
                        node.Wins += result;
                        node = node.Parent!;
                    }
                }
            }

            var children = new List<MCTSNode>(root.Children);
            CleanupTree(root);
            return children;
        }

        private static void CleanupTree(MCTSNode node)
        {
            node.State?.ReturnToPool();
            node.State = null;
            foreach (var child in node.Children)
            {
                CleanupTree(child);
            }
            node.Children.Clear();
        }

        private void EnsureState(MCTSNode node)
        {
            if (node.State != null)
                return;
            var parent = node.Parent!;
            EnsureState(parent);
            node.State = parent.State!.DeepCopy();
            var pa = node.PlayerAction!.Value;
            var ea = node.Action!.Value;
            node.State.Declare(pa.Item1, pa.Item2, ea.Item1, ea.Item2, pa.Item3, ea.Item3);
        }

        private (string, int, string) SampleFromTopK(List<MCTSNode> children, int round)
        {
            int k = Math.Min(2, children.Count);
            double temperature = Math.Max(0.003, _params.TemperatureCoefficient * round);


            children.ForEach(c =>
            {
                if (c.Action.HasValue)
                {
                    var action = c.Action.Value;
                    Console.WriteLine($"{action.skill}:{c.Wins / (c.Visits + 1e-6)}");
                }
            });
            Console.WriteLine($"");

            var topK = children
                .OrderByDescending(c => c.Wins / (c.Visits + 1e-6))
                .Take(k)
                .ToList();

            double maxScore = topK.Max(c => c.Wins / (c.Visits + 1e-6));

            List<double> weights = new();
            double sum = 0;
            foreach (var c in topK)
            {
                double q = c.Wins / (c.Visits + 1e-6);
                double w = Math.Exp((q - maxScore) / temperature);
                weights.Add(w);
                sum += w;
            }

            double r = _random.Value!.NextDouble() * sum;
            double acc = 0;
            for (int i = 0; i < topK.Count; i++)
            {
                acc += weights[i];
                if (r <= acc)
                    return topK[i].Action!.Value;
            }
            return topK.Last().Action!.Value;
        }

        private class MCTSNode
        {
            public GameInstance? State;
            public MCTSNode? Parent;
            public List<MCTSNode> Children = new();
            public (string skill, int param, string stringParam)? Action;
            public (string skill, int param, string stringParam)? PlayerAction;
            public int Visits = 0;
            public double Wins = 0;
            public List<(string, int, string)> UntriedActions;

            public MCTSNode(GameInstance? state, MCTSNode? parent, List<(string, int, string)> actions)
            {
                State = state;
                Parent = parent;
                UntriedActions = new List<(string, int, string)>(actions.Count > 0 ? actions.Count : 16);
                if (actions.Count > 0)
                    UntriedActions.AddRange(actions);
            }
        }

        private MCTSNode Select(MCTSNode node)
        {
            return node.Children.OrderByDescending(child =>
            {
                double mean = child.Wins / (child.Visits + 1e-6);
                double uct = mean +
                    MathF.Sqrt(2) * Math.Sqrt(Math.Log(node.Visits + 1) / (child.Visits + 1e-6));
                return uct;
            }).First();
        }

        private double Evaluate(GameInstance state)
        {
            var enemy = state.Enemy;
            var player = state.Player;

            double enemyHP = enemy.Focus.Get<Health>().HP;
            double playerHP = player.Focus.Get<Health>().HP;

            double enemyIron = enemy.Focus.Get<Resource>().QueryAll(ResourceType.Instance.Iron());
            double enemySpace = enemy.Focus.Get<Resource>().QueryAll(ResourceType.Instance.Space());
            double enemyTime = enemy.Focus.Get<Resource>().QueryAll(ResourceType.Instance.Time());
            double enemySpecific = enemy.Focus.Get<Resource>().QuerySpecific();

            double playerIron = player.Focus.Get<Resource>().QueryAll(ResourceType.Instance.Iron());
            double playerSpace = player.Focus.Get<Resource>().QueryAll(ResourceType.Instance.Space());
            double playerSpecific = player.Focus.Get<Resource>().QuerySpecific();

            bool haveProfession = enemy.Focus.Get<Skill>().HaveProfession;
            bool playerHaveProfession = player.Focus.Get<Skill>().HaveProfession;

            int round = state.History.SkillHistory.Count;//已经过的回合数

            // 终局
            if (enemyHP <= 0) return -_params.WinScore;
            if (playerHP <= 0) return _params.WinScore;

            double score = 0;

            // 阶段划分
            bool early = round < 8;
            bool mid = round >= 8 && round < 15;
            bool late = round >= 15;

            // 1️⃣ 资源系统
            double resourceScore = 0;
            if (early)
            {
                resourceScore += enemyIron * _params.EarlyIronWeight;
                resourceScore += Math.Max(0, enemyIron - 4) * _params.EarlyExcessIronWeight;
                resourceScore += enemySpace * _params.EarlySpaceWeight;
                resourceScore += enemyTime * _params.EarlyTimeWeight;
                resourceScore += enemySpecific * _params.EarlyDefaultWeight;
            }
            else if (mid)
            {
                resourceScore += enemyIron * _params.MidIronWeight;
                resourceScore += enemySpace * _params.MidSpaceWeight;
                resourceScore += enemyTime * _params.MidTimeWeight;
                resourceScore += enemySpecific * _params.MidDefaultWeight;
            }
            else // late
            {
                resourceScore += enemyIron * _params.LateIronWeight;
                resourceScore += enemySpace * _params.LateSpaceWeight;
                resourceScore += enemyTime * _params.LateTimeWeight;
                resourceScore += enemySpecific * _params.LateDefaultWeight;
            }
            score += resourceScore;

            // 3️⃣ 攻击策略
            if (!early)
            {
                double hpDiff = enemyHP - playerHP;
                if (!haveProfession)
                {
                    if (early)
                        score -= (10 - playerHP) * _params.EarlyUnnecessaryAttackPenaltyMultiplier;
                    else if (mid)
                    {
                        if (hpDiff > _params.HpAdvantageThreshold)
                            score += hpDiff * _params.MidAdvantageAttackBonusMultiplier;
                        else
                            score -= (10 - playerHP) * _params.MidUnnecessaryAttackPenaltyMultiplier;
                    }
                }
                else
                {
                    score += (10 - playerHP) * _params.WithProfessionDamageBonusMultiplier;
                    score += hpDiff * _params.WithProfessionHpDiffBonusMultiplier;
                }
            }

            // 4️⃣ 回合节奏
            if (late)
                score -= round * _params.LateRoundPenaltyPerRound;

            const double scale = 30.0;

            double normalized =
                _params.WinScore * Math.Tanh(score / scale);

            return normalized;
        }

        private bool IsTerminal(GameInstance state)
        {
            return state.Enemy.Focus.Get<Health>().HP <= 0 ||
                    state.Player.Focus.Get<Health>().HP <= 0;
        }

        private (string, int, string) RandomAction(Community actor, GameInstance instance)
        {
            var actions = GetAllAvailable(actor, instance);
            if (actions.Count == 0)
                return ("", 0, "");
            return actions[_random.Value!.Next(actions.Count)];
        }

        private List<(string, int, string)> GetAllAvailable(Community actor, GameInstance instance)
        {
            List<(string, int, string)> res = new(16);
            var names = actor.Focus.Get<Skill>().GetAvailableSkillNames();

            foreach (var name in names)
            {
                if (_uselessActions.Contains(name))
                    continue;

                for (int i = 0; i <= 5; i++)
                {
                    if (name != "magicattack" && name != "spaceattack" && i > 0)
                        break;

                    SkillDeclareResult r = (actor == instance.Player)
                        ? instance.TryDeclare(name, i)
                        : instance.ETryDeclare(name, i);

                    if (r == SkillDeclareResult.Success)
                        res.Add((name, i, ""));
                    else if (i > 0)
                        break;
                }
            }
            return res;
        }
    }
}