using System.Diagnostics;
using System.Text.Json.Serialization;
using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapModels.Components;


namespace BlacksmithCore.AI.Strategies
{
    // 与 GeneralStrategyParams 结构兼容，新增 OpponentGreedyRate / OpponentDepth / MctsIterations。
    // 故意不与 GeneralStrategyParams 共用，以保证 GeneralStrategy 的既有行为完全冻结。
    public class AdversarialStrategyParams
    {
        [JsonConstructor]
        public AdversarialStrategyParams()
        {
        }

        public AdversarialStrategyParams(AdversarialStrategyParams other)
        {
            TemperatureCoefficient = other.TemperatureCoefficient;
            WinScore = other.WinScore;
            LoseScore = other.LoseScore;
            PlayerResourceEnemyHPRatio = other.PlayerResourceEnemyHPRatio;
            EnemyResourcePlayerHPRatio = other.EnemyResourcePlayerHPRatio;
            EarlyIronWeight = other.EarlyIronWeight;
            EarlyExcessIronWeight = other.EarlyExcessIronWeight;
            EarlySpaceWeight = other.EarlySpaceWeight;
            EarlyTimeWeight = other.EarlyTimeWeight;
            EarlyDefaultWeight = other.EarlyDefaultWeight;
            EarlyIronOverstockPenalty = other.EarlyIronOverstockPenalty;
            MidIronWeight = other.MidIronWeight;
            MidSpaceWeight = other.MidSpaceWeight;
            MidTimeWeight = other.MidTimeWeight;
            MidDefaultWeight = other.MidDefaultWeight;
            LateIronWeight = other.LateIronWeight;
            LateSpaceWeight = other.LateSpaceWeight;
            LateTimeWeight = other.LateTimeWeight;
            LateDefaultWeight = other.LateDefaultWeight;
            HaveProfessionBonus = other.HaveProfessionBonus;
            IronDeficitPenaltyWhenEnemyHasProfession = other.IronDeficitPenaltyWhenEnemyHasProfession;
            IronDeficitPenaltyWhenBothNoProfession = other.IronDeficitPenaltyWhenBothNoProfession;
            IronDeficitThreshold = other.IronDeficitThreshold;
            EarlyUnnecessaryAttackPenaltyMultiplier = other.EarlyUnnecessaryAttackPenaltyMultiplier;
            MidAdvantageAttackBonusMultiplier = other.MidAdvantageAttackBonusMultiplier;
            MidUnnecessaryAttackPenaltyMultiplier = other.MidUnnecessaryAttackPenaltyMultiplier;
            WithProfessionDamageBonusMultiplier = other.WithProfessionDamageBonusMultiplier;
            WithProfessionHpDiffBonusMultiplier = other.WithProfessionHpDiffBonusMultiplier;
            HpAdvantageThreshold = other.HpAdvantageThreshold;
            EarlyRoundBonusPerRound = other.EarlyRoundBonusPerRound;
            LateRoundPenaltyPerRound = other.LateRoundPenaltyPerRound;
            OpponentGreedyRate = other.OpponentGreedyRate;
            OpponentDepth = other.OpponentDepth;
            MctsIterations = other.MctsIterations;
        }

        public AdversarialStrategyParams GetMutation(Random rand, double MutationScale)
        {
            double Mut(double v)
            {
                double noise = (rand.NextDouble() * 2 - 1);
                return v * (1 + noise * MutationScale);
            }
            AdversarialStrategyParams res = new(this);
            res.TemperatureCoefficient = Mut(TemperatureCoefficient);
            res.WinScore = Mut(WinScore);
            res.LoseScore = Mut(LoseScore);
            res.PlayerResourceEnemyHPRatio = Mut(PlayerResourceEnemyHPRatio);
            res.EnemyResourcePlayerHPRatio = Mut(EnemyResourcePlayerHPRatio);
            res.EarlyIronWeight = Mut(EarlyIronWeight);
            res.EarlyExcessIronWeight = Mut(EarlyExcessIronWeight);
            res.EarlySpaceWeight = Mut(EarlySpaceWeight);
            res.EarlyTimeWeight = Mut(EarlyTimeWeight);
            res.EarlyDefaultWeight = Mut(EarlyDefaultWeight);
            res.EarlyIronOverstockPenalty = Mut(EarlyIronOverstockPenalty);
            res.MidIronWeight = Mut(MidIronWeight);
            res.MidSpaceWeight = Mut(MidSpaceWeight);
            res.MidTimeWeight = Mut(MidTimeWeight);
            res.MidDefaultWeight = Mut(MidDefaultWeight);
            res.LateIronWeight = Mut(LateIronWeight);
            res.LateSpaceWeight = Mut(LateSpaceWeight);
            res.LateTimeWeight = Mut(LateTimeWeight);
            res.LateDefaultWeight = Mut(LateDefaultWeight);
            res.HaveProfessionBonus = Mut(HaveProfessionBonus);
            res.IronDeficitPenaltyWhenEnemyHasProfession = Mut(IronDeficitPenaltyWhenEnemyHasProfession);
            res.IronDeficitPenaltyWhenBothNoProfession = Mut(IronDeficitPenaltyWhenBothNoProfession);
            res.IronDeficitThreshold = Mut(IronDeficitThreshold);
            res.EarlyUnnecessaryAttackPenaltyMultiplier = Mut(EarlyUnnecessaryAttackPenaltyMultiplier);
            res.MidAdvantageAttackBonusMultiplier = Mut(MidAdvantageAttackBonusMultiplier);
            res.MidUnnecessaryAttackPenaltyMultiplier = Mut(MidUnnecessaryAttackPenaltyMultiplier);
            res.WithProfessionDamageBonusMultiplier = Mut(WithProfessionDamageBonusMultiplier);
            res.WithProfessionHpDiffBonusMultiplier = Mut(WithProfessionHpDiffBonusMultiplier);
            res.HpAdvantageThreshold = Mut(HpAdvantageThreshold);
            res.EarlyRoundBonusPerRound = Mut(EarlyRoundBonusPerRound);
            res.LateRoundPenaltyPerRound = Mut(LateRoundPenaltyPerRound);
            // 仅 OpponentGreedyRate 参与变异并裁剪到合法区间；OpponentDepth / MctsIterations 是离散预算，留给运行时手调。
            res.OpponentGreedyRate = Math.Clamp(Mut(OpponentGreedyRate), 0.0, 1.0);
            res.OpponentDepth = OpponentDepth;
            res.MctsIterations = MctsIterations;
            return res;
        }

        public AdversarialStrategyParams GetCrossWith(Random rand, AdversarialStrategyParams other)
        {
            double Pick(double a, double b) => rand.NextDouble() < 0.5 ? a : b;
            AdversarialStrategyParams res = new();
            res.TemperatureCoefficient = Pick(TemperatureCoefficient, other.TemperatureCoefficient);
            res.WinScore = Pick(WinScore, other.WinScore);
            res.LoseScore = Pick(LoseScore, other.LoseScore);
            res.PlayerResourceEnemyHPRatio = Pick(PlayerResourceEnemyHPRatio, other.PlayerResourceEnemyHPRatio);
            res.EnemyResourcePlayerHPRatio = Pick(EnemyResourcePlayerHPRatio, other.EnemyResourcePlayerHPRatio);
            res.EarlyIronWeight = Pick(EarlyIronWeight, other.EarlyIronWeight);
            res.EarlyExcessIronWeight = Pick(EarlyExcessIronWeight, other.EarlyExcessIronWeight);
            res.EarlySpaceWeight = Pick(EarlySpaceWeight, other.EarlySpaceWeight);
            res.EarlyTimeWeight = Pick(EarlyTimeWeight, other.EarlyTimeWeight);
            res.EarlyDefaultWeight = Pick(EarlyDefaultWeight, other.EarlyDefaultWeight);
            res.EarlyIronOverstockPenalty = Pick(EarlyIronOverstockPenalty, other.EarlyIronOverstockPenalty);
            res.MidIronWeight = Pick(MidIronWeight, other.MidIronWeight);
            res.MidSpaceWeight = Pick(MidSpaceWeight, other.MidSpaceWeight);
            res.MidTimeWeight = Pick(MidTimeWeight, other.MidTimeWeight);
            res.MidDefaultWeight = Pick(MidDefaultWeight, other.MidDefaultWeight);
            res.LateIronWeight = Pick(LateIronWeight, other.LateIronWeight);
            res.LateSpaceWeight = Pick(LateSpaceWeight, other.LateSpaceWeight);
            res.LateTimeWeight = Pick(LateTimeWeight, other.LateTimeWeight);
            res.LateDefaultWeight = Pick(LateDefaultWeight, other.LateDefaultWeight);
            res.HaveProfessionBonus = Pick(HaveProfessionBonus, other.HaveProfessionBonus);
            res.IronDeficitPenaltyWhenEnemyHasProfession = Pick(IronDeficitPenaltyWhenEnemyHasProfession, other.IronDeficitPenaltyWhenEnemyHasProfession);
            res.IronDeficitPenaltyWhenBothNoProfession = Pick(IronDeficitPenaltyWhenBothNoProfession, other.IronDeficitPenaltyWhenBothNoProfession);
            res.IronDeficitThreshold = Pick(IronDeficitThreshold, other.IronDeficitThreshold);
            res.EarlyUnnecessaryAttackPenaltyMultiplier = Pick(EarlyUnnecessaryAttackPenaltyMultiplier, other.EarlyUnnecessaryAttackPenaltyMultiplier);
            res.MidAdvantageAttackBonusMultiplier = Pick(MidAdvantageAttackBonusMultiplier, other.MidAdvantageAttackBonusMultiplier);
            res.MidUnnecessaryAttackPenaltyMultiplier = Pick(MidUnnecessaryAttackPenaltyMultiplier, other.MidUnnecessaryAttackPenaltyMultiplier);
            res.WithProfessionDamageBonusMultiplier = Pick(WithProfessionDamageBonusMultiplier, other.WithProfessionDamageBonusMultiplier);
            res.WithProfessionHpDiffBonusMultiplier = Pick(WithProfessionHpDiffBonusMultiplier, other.WithProfessionHpDiffBonusMultiplier);
            res.HpAdvantageThreshold = Pick(HpAdvantageThreshold, other.HpAdvantageThreshold);
            res.EarlyRoundBonusPerRound = Pick(EarlyRoundBonusPerRound, other.EarlyRoundBonusPerRound);
            res.LateRoundPenaltyPerRound = Pick(LateRoundPenaltyPerRound, other.LateRoundPenaltyPerRound);
            res.OpponentGreedyRate = Pick(OpponentGreedyRate, other.OpponentGreedyRate);
            res.OpponentDepth = OpponentDepth;
            res.MctsIterations = MctsIterations;
            return res;
        }

        public double TemperatureCoefficient = 0.03;

        public double WinScore = 1e9;
        public double LoseScore = -1e9;

        public double PlayerResourceEnemyHPRatio = 100;
        public double EnemyResourcePlayerHPRatio = 100;

        public double EarlyIronWeight = 1200;
        public double EarlyExcessIronWeight = 1200;
        public double EarlySpaceWeight = 4000;
        public double EarlyTimeWeight = 3500;
        public double EarlyDefaultWeight = 2000;
        public double EarlyIronOverstockPenalty = 80;

        public double MidIronWeight = 300;
        public double MidSpaceWeight = 1000;
        public double MidTimeWeight = 900;
        public double MidDefaultWeight = 600;

        public double LateIronWeight = 100;
        public double LateSpaceWeight = 250;
        public double LateTimeWeight = 250;
        public double LateDefaultWeight = 150;

        public double HaveProfessionBonus = 500;
        public double IronDeficitPenaltyWhenEnemyHasProfession = 300;
        public double IronDeficitPenaltyWhenBothNoProfession = 1000;
        public double IronDeficitThreshold = 3;

        public double EarlyUnnecessaryAttackPenaltyMultiplier = 30;
        public double MidAdvantageAttackBonusMultiplier = 2;
        public double MidUnnecessaryAttackPenaltyMultiplier = 10;
        public double WithProfessionDamageBonusMultiplier = 20;
        public double WithProfessionHpDiffBonusMultiplier = 5;
        public double HpAdvantageThreshold = 20;

        public double EarlyRoundBonusPerRound = 1;
        public double LateRoundPenaltyPerRound = 40;

        // ========== Adversarial 专属 ==========
        // 启发式对手在每层递归里走 greedy（穷举+前看+argmax）的概率。
        // 1 - OpponentGreedyRate 即 ε-greedy 中的 ε（随机短路概率），用来防止搜索过窄、避免对手被 100% 规则化。
        // 例：0.9 表示每层有 90% 概率走完整 greedy 推演、10% 概率直接 RandomAction。
        public double OpponentGreedyRate = 0.9;
        // 启发式前看深度：0=纯随机；1=一步前看（对手随机反应）；2=两步前看（对手也做一次一步前看，近似 depth-2 minimax）
        public int OpponentDepth = 2;
        // MCTS 外层总迭代数（会被均分到线程）
        public int MctsIterations = 4000;
    }

    /// <summary>
    /// 在 GeneralStrategy 的 MCTS 骨架上替换 Expansion/Rollout 里的对手动作生成器为
    /// 「ε-greedy + N 步前看」的递归启发式，把"对手会针对我"显式注入到搜索过程。
    /// 与 GeneralStrategy 完全独立，便于 A/B 对比。
    /// </summary>
    public class AdversarialStrategy : IAIStrategy
    {
        private readonly AdversarialStrategyParams _params;
        private GameInstance _main = null!;
        private static ThreadLocal<Random> _random = new(() =>
        {
            return new Random(Random.Shared.Next());
        });

        public string Name => "Adversarial";

        public AdversarialStrategy()
        {
            _params = new AdversarialStrategyParams();
        }

        public void Init(GameInstance gameInstance)
        {
            _main = gameInstance;
        }

        public (string skillName, int param, string stringParam) ChooseSkill()
        {
            var sw = Stopwatch.StartNew();

            int threadCount = Math.Min(7, Environment.ProcessorCount);
            int iterationsPerThread = Math.Max(1, 70 / threadCount);
            var tasks = new List<Task<List<MCTSNode>>>();

            for (int t = 0; t < threadCount; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var localGame = _main.DeepCopy();
                    return RunMCTS(localGame, iterationsPerThread);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // 根并行：把各线程 root 的孩子按 (skill, param) 聚合
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

            var result = SampleFromTopK(finalChildren, _main.History.SkillHistory.Count);

            sw.Stop();
            Console.WriteLine(
                $"[Adversarial] depth={_params.OpponentDepth} iter={_params.MctsIterations} "
                + $"耗时={sw.ElapsedMilliseconds}ms 选择={result.Item1}/{result.Item2}/{result.Item3}");

            return result;
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

                // Expansion：自己仍从 UntriedActions 取（MCTS 展开的本意），对手用启发式
                if (node.UntriedActions.Count > 0)
                {
                    var action = node.UntriedActions[0];
                    node.UntriedActions.RemoveAt(0);

                    var nextState = node.State.DeepCopy();
                    var playerAction = HeuristicAction(nextState.Player, nextState, _params.OpponentDepth);
                    nextState.Declare(
                        playerAction.Item1, playerAction.Item2,
                        action.Item1, action.Item2,
                        playerAction.Item3, action.Item3
                    );

                    var nextActions = GetAllAvailable(nextState.Enemy, nextState);
                    var child = new MCTSNode(nextState, node, nextActions) { Action = action };
                    node.Children.Add(child);
                    node = child;
                }

                // Rollout：双方都用同一深度的启发式，让推演到「双方都理性」的局面
                var simState = node.State.DeepCopy();
                for (int d = 0; d < 5; d++)
                {
                    if (IsTerminal(simState))
                        break;

                    var p = HeuristicAction(simState.Player, simState, _params.OpponentDepth);
                    var e = HeuristicAction(simState.Enemy, simState, _params.OpponentDepth);
                    simState.Declare(p.Item1, p.Item2, e.Item1, e.Item2, p.Item3, e.Item3);
                }

                double result = Evaluate(simState);
                simState.ReturnToPool();

                // Backprop
                while (node != null)
                {
                    node.Visits++;
                    node.Wins += result;
                    node = node.Parent!;
                }
            }

            var children = new List<MCTSNode>(root.Children);
            CleanupTree(root);
            return children;
        }

        private static void CleanupTree(MCTSNode node)
        {
            node.State?.ReturnToPool();
            node.State = null!;
            foreach (var child in node.Children)
            {
                CleanupTree(child);
            }
            node.Children.Clear();
        }

        /// <summary>
        /// ε-greedy + depth 步前看的启发式动作选择。
        /// depth=0 退化为均匀随机；depth>=1 时遍历自己所有合法动作，让对手用 depth-1 递归选择反制，
        /// 然后用同一份 Evaluate 给结果局面打分。
        /// 视角对齐：Evaluate 是 Enemy 视角，actor=Player 时取负。
        /// </summary>
        private (string, int, string) HeuristicAction(Community actor, GameInstance instance, int depth)
        {
            if (depth <= 0)
                return RandomAction(actor, instance);

            if (_random.Value!.NextDouble() >= _params.OpponentGreedyRate)
                return RandomAction(actor, instance);

            var actions = GetAllAvailable(actor, instance);
            if (actions.Count == 0)
                return ("", 0, "");

            bool actorIsEnemy = (actor == instance.Enemy);
            double bestScore = double.NegativeInfinity;
            (string, int, string) bestAction = actions[0];

            foreach (var a in actions)
            {
                GameInstance sim = instance.DeepCopy();

                var opp = actorIsEnemy ? sim.Player : sim.Enemy;
                var oppAction = HeuristicAction(opp, sim, depth - 1);

                if (actorIsEnemy)
                    sim.Declare(oppAction.Item1, oppAction.Item2, a.Item1, a.Item2, oppAction.Item3, a.Item3);
                else
                    sim.Declare(a.Item1, a.Item2, oppAction.Item1, oppAction.Item2, a.Item3, oppAction.Item3);

                double score = Evaluate(sim);
                if (!actorIsEnemy) score = -score;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = a;
                }

                sim.ReturnToPool();
            }
            return bestAction;
        }

        private (string, int, string) SampleFromTopK(List<MCTSNode> children, int round)
        {
            if (children.Count == 0)
                return ("", 0, "");

            int k = Math.Min(2, children.Count);
            double temperature = Math.Max(0, _params.TemperatureCoefficient * round);

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
            public GameInstance State;
            public MCTSNode? Parent;
            public List<MCTSNode> Children = new();
            public (string skill, int param, string stringParam)? Action;
            public int Visits = 0;
            public double Wins = 0;
            public List<(string, int, string)> UntriedActions;

            public MCTSNode(GameInstance state, MCTSNode? parent, List<(string, int, string)> actions)
            {
                State = state;
                Parent = parent;
                UntriedActions = new List<(string, int, string)>(actions);
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

        // 与 GeneralStrategy.Evaluate 完全一致（Enemy 视角），保持 A/B 可比性
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

            int round = state.History.SkillHistory.Count;

            if (enemyHP <= 0) return _params.LoseScore;
            if (playerHP <= 0) return _params.WinScore;

            double score = 0;

            bool early = round < 10;
            bool mid = round >= 10 && round < 15;
            bool late = round >= 15;

            score -= 10 *
                ((playerIron + 3 * playerSpace + 2 * playerSpecific) / (enemyHP + 1e-6)) *
                    _params.PlayerResourceEnemyHPRatio;

            score += 10 *
                ((enemyIron + 3 * enemySpace + 2 * enemySpecific) / (playerHP + 1e-6)) *
                    _params.EnemyResourcePlayerHPRatio;

            double resourceScore = 0;
            if (early)
            {
                resourceScore += enemyIron * _params.EarlyIronWeight;
                resourceScore += Math.Max(0, enemyIron - 4) * _params.EarlyExcessIronWeight;
                resourceScore += enemySpace * _params.EarlySpaceWeight;
                resourceScore += enemyTime * _params.EarlyTimeWeight;
                resourceScore += enemySpecific * _params.EarlyDefaultWeight;
                resourceScore -= Math.Max(0, enemyIron - 7) * _params.EarlyIronOverstockPenalty;
            }
            else if (mid)
            {
                resourceScore += enemyIron * _params.MidIronWeight;
                resourceScore += enemySpace * _params.MidSpaceWeight;
                resourceScore += enemyTime * _params.MidTimeWeight;
                resourceScore += enemySpecific * _params.MidDefaultWeight;
            }
            else
            {
                resourceScore += enemyIron * _params.LateIronWeight;
                resourceScore += enemySpace * _params.LateSpaceWeight;
                resourceScore += enemyTime * _params.LateTimeWeight;
                resourceScore += enemySpecific * _params.LateDefaultWeight;
            }
            score += resourceScore;

            if (haveProfession)
                score += _params.HaveProfessionBonus;

            if (playerHaveProfession && !haveProfession)
            {
                if (enemyIron - playerIron < _params.IronDeficitThreshold)
                    score -= _params.IronDeficitPenaltyWhenEnemyHasProfession;
            }

            if (!playerHaveProfession && !haveProfession)
            {
                if (enemyIron - playerIron < 0)
                    score -= _params.IronDeficitPenaltyWhenBothNoProfession;
            }

            double hpDiff = enemyHP - playerHP;
            if (!haveProfession)
            {
                if (early)
                    score -= (100 - playerHP) * _params.EarlyUnnecessaryAttackPenaltyMultiplier;
                else if (mid)
                {
                    if (hpDiff > _params.HpAdvantageThreshold)
                        score += hpDiff * _params.MidAdvantageAttackBonusMultiplier;
                    else
                        score -= (100 - playerHP) * _params.MidUnnecessaryAttackPenaltyMultiplier;
                }
            }
            else
            {
                score += (100 - playerHP) * _params.WithProfessionDamageBonusMultiplier;
                score += hpDiff * _params.WithProfessionHpDiffBonusMultiplier;
            }

            if (early)
                score += round * _params.EarlyRoundBonusPerRound;
            else if (late)
                score -= round * _params.LateRoundPenaltyPerRound;

            return score;
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
            List<(string, int, string)> res = new();
            var names = actor.Focus.Get<Skill>().GetAvailableSkillNames();

            foreach (var name in names)
            {
                var useless = new List<string>() { "stick", "drill", "recovery", "shield", "thornshield", "mute" };
                if (useless.Contains(name))
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
