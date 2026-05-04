using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Judgement.Core;
using ClapInfra.ClapJudgement;

namespace BlacksmithCore.Infra.Models.Judgement
{
    public class JudgeRuleManager : ClapJudgeRuleManager<Community>
    {
        DynamicJudgeRulePool _dynamicPool = new();
        public class StageRuleContainer
        {
            public class RuleUnit
            {
                public int RemainingRounds;
                public int DelayRounds;
                public Action<Community, Community> Rule;
                public void TimePass()
                {
                    if (DelayRounds > 0)
                    {
                        DelayRounds--;
                    }
                    else
                    {
                        RemainingRounds--;
                    }
                }
                public RuleUnit(int remainingRounds, int delayRounds, Action<Community, Community> rule)
                {
                    RemainingRounds = remainingRounds;
                    DelayRounds = delayRounds;
                    Rule = rule;
                }
            }
            private readonly Action<Community, Community> _baseRule;
            private readonly List<RuleUnit> _overrideRules = new();
            public readonly List<RuleUnit> _modifiersBefore = new();
            public readonly List<RuleUnit> _modifiersAfter = new();
            public StageRuleContainer(Action<Community, Community> baseRule)
            {
                _baseRule = baseRule;
            }
            public void AddOverride(RuleUnit ruleUnit)
            {
                _overrideRules.Add(ruleUnit);
            }
            public void AddModifier(RuleUnit ruleUnit, ModifierOrder modifierOrder)
            {
                if (modifierOrder == ModifierOrder.Before)
                {
                    _modifiersBefore.Add(ruleUnit);
                }
                else
                {
                    _modifiersAfter.Add(ruleUnit);
                }
            }
            public void Update()
            {
                _overrideRules.RemoveAll(o => o.RemainingRounds <= 0);
                _modifiersBefore.RemoveAll(o => o.RemainingRounds <= 0);
                _modifiersAfter.RemoveAll(o => o.RemainingRounds <= 0);

                _overrideRules.ForEach(o => o.TimePass());
                _modifiersBefore.ForEach(o => o.TimePass());
                _modifiersAfter.ForEach(o => o.TimePass());
            }
            public Action<Community, Community> Build()
            {
                Update();
                var overrideRules = _overrideRules
                    .Where(m => m.RemainingRounds == 0)
                    .Select(m => m.Rule)
                    .ToList();
                var modifiersBefore = _modifiersBefore
                    .Where(m => m.RemainingRounds == 0)
                    .Select(m => m.Rule)
                    .ToList();
                var modifiersAfter = _modifiersAfter
                    .Where(m => m.RemainingRounds == 0)
                    .Select(m => m.Rule)
                    .ToList();
                var overrideRule = overrideRules.Count > 0 ? overrideRules[^1] : null;
                return (player, enemy) =>
                {
                    // BEFORE modifiers
                    foreach (var rule in modifiersBefore)
                    {
                        rule(player, enemy);
                    }
                    // 核心规则
                    var core = overrideRule ?? _baseRule;
                    core(player, enemy);
                    // AFTER modifiers
                    foreach (var rule in modifiersAfter)
                    {
                        rule(player, enemy);
                    }
                };
            }
        }

        private readonly SortedDictionary<JudgeStage.BEValue, StageRuleContainer> _ruleContainers = new()
        {
            {
                JudgeStage.Instance.OnBegin(),
                new((player, enemy) => { })
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterResolutionWritten(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterResolutionWritten(), player, enemy))
            },
            {
                JudgeStage.Instance.OnEffectSwaping(),
                new(SwapEffects)
            },
            {
                JudgeStage.Instance.OnAttackCanceling(),
                new(CancelAttacks)
            },
            {
                JudgeStage.Instance.OnAttackSwaping(),
                new(SwapAttacks)
            },
            {
                JudgeStage.Instance.OnApplyingEffect(),
                new(ApplyEffect)
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterTransport(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterTransport(), player, enemy))
            },
            {
                JudgeStage.Instance.OnApplyingOthers(),
                new(ApplyOthers)
            },
            {
                JudgeStage.Instance.OnUpdating(),
                new(Update)
            },
            {
                JudgeStage.Instance.OnEffectTaking_AfterResult(),
                new((player, enemy) => TakeEffects(EffectType.Instance.AfterResult(), player, enemy))
            },
            {
                JudgeStage.Instance.OnEnd(),
                new((player, enemy) => { })
            }
        };
        #region Default Rules（原有逻辑）
        private static void TakeEffects(EffectType.BEValue type, Community player, Community enemy)
        {
            foreach (var temp in player.ActorList)
            {
                temp.Get<Effect>().Execute(type, temp);
            }

            foreach (var temp in enemy.ActorList)
            {
                temp.Get<Effect>().Execute(type, temp);
            }
        }

        private static void SwapEffects(Community player, Community enemy)
        {
            SwapEffects(player.Focus.Get<TurnContext>().Get<EffectResolution>(),
                        enemy.Focus.Get<TurnContext>().Get<EffectResolution>());
        }

        private static void SwapEffects(List<EffectResolution> playerResolutions,
            List<EffectResolution> enemyResolutions)
        {
            var playerTemp = playerResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() || e.DelayRounds == 0).ToList();
            var enemyTemp = enemyResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() || e.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(e => playerTemp.Contains(e));
            enemyResolutions.RemoveAll(e => enemyTemp.Contains(e));

            playerResolutions.AddRange(enemyTemp);
            enemyResolutions.AddRange(playerTemp);
        }

        private static void CancelAttacks(Community player, Community enemy)
        {
            CancelAttackResolutions(player.Focus.Get<TurnContext>().Get<AttackResolution>(),
                                    enemy.Focus.Get<TurnContext>().Get<AttackResolution>());
        }

        private static void CancelAttackResolutions(List<AttackResolution> playerResolutions,
            List<AttackResolution> enemyResolutions)
        {
            playerResolutions.OrderBy(a => a.Type);
            enemyResolutions.OrderBy(a => a.Type);
            int playerIndex = 0;
            int enemyIndex = 0;

            while (playerIndex < playerResolutions.Count && enemyIndex < enemyResolutions.Count)
            {
                var playerAttack = playerResolutions[playerIndex];
                var enemyAttack = enemyResolutions[enemyIndex];

                if (playerAttack.Type == AttackType.Instance.Real() || playerAttack.DelayRounds != 0)
                {
                    playerIndex++;
                    continue;
                }

                if (enemyAttack.Type == AttackType.Instance.Real() || enemyAttack.DelayRounds != 0)
                {
                    enemyIndex++;
                    continue;
                }

                (playerAttack.Power, enemyAttack.Power) =
                    Cancel(playerAttack.Power, enemyAttack.Power);

                if (playerAttack.Power <= 0f)
                    playerResolutions.RemoveAt(playerIndex);

                if (enemyAttack.Power <= 0f)
                    enemyResolutions.RemoveAt(enemyIndex);
            }
        }
        public static (float, float) Cancel(float a, float b)
        {
            return (MathF.Max(0, a - b), MathF.Max(0, b - a));
        }
        private static void SwapAttacks(Community player, Community enemy)
        {
            SwapAttacks(player.Focus.Get<TurnContext>().Get<AttackResolution>(),
                        enemy.Focus.Get<TurnContext>().Get<AttackResolution>());
        }

        private static void SwapAttacks(List<AttackResolution> playerResolutions,
            List<AttackResolution> enemyResolutions)
        {
            var playerTemp = playerResolutions.Where(e => e.DelayRounds == 0).ToList();
            var enemyTemp = enemyResolutions.Where(e => e.DelayRounds == 0).ToList();

            playerResolutions.RemoveAll(e => playerTemp.Contains(e));
            enemyResolutions.RemoveAll(e => enemyTemp.Contains(e));

            playerResolutions.AddRange(enemyTemp);
            enemyResolutions.AddRange(playerTemp);
        }
        private static void ApplyEffect(Community player, Community enemy)
        {
            player.Focus.Get<TurnContext>().Execute<EffectResolution>(player);
            enemy.Focus.Get<TurnContext>().Execute<EffectResolution>(enemy);
        }
        private static void ApplyOthers(Community player, Community enemy)
        {
            player.Focus.Get<TurnContext>().Execute<DefenseResolution>(player);
            enemy.Focus.Get<TurnContext>().Execute<DefenseResolution>(enemy);

            player.Focus.Get<TurnContext>().Execute<AttackResolution>(player);
            enemy.Focus.Get<TurnContext>().Execute<AttackResolution>(enemy);

            player.Focus.Get<TurnContext>().Execute<ResourceResolution>(player);
            enemy.Focus.Get<TurnContext>().Execute<ResourceResolution>(enemy);
        }

        private static void Update(Community player, Community enemy)
        {
            foreach (var temp in player.ActorList)
                temp.Update();

            foreach (var temp in enemy.ActorList)
                temp.Update();
        }

        #endregion
        public override Action<Community, Community> GetRule()
        {
            Action<Community, Community> result = (a, b) => { };

            foreach (var stage in _ruleContainers.OrderBy(k => k.Key))
            {
                result += stage.Value.Build();
            }

            return result;
        }
        public void RegistJudgeRuleDynamic(DynamicJudgeRuleName.BEValue name, List<Mutation> mutations)
        {
            _dynamicPool.RegistDynamic(name, mutations);
        }
        public void AddJudgeRule(Community source, DynamicJudgeRuleName.BEValue name)
        {
            List<Mutation> mutations = _dynamicPool.Query(source, name);
            foreach (var mutation in mutations)
            {
                StageRuleContainer.RuleUnit unit = new(mutation.RemainingRounds, mutation.DelayRounds, mutation.JudgeRule);
                if (mutation.RuleType == RuleType.Override)
                {
                    _ruleContainers[mutation.Stage].AddOverride(unit);
                }
                else
                {
                    _ruleContainers[mutation.Stage].AddModifier(unit, mutation.ModifierOrder);
                }
            }
        }
    }
}
