using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Judgement.Core;
using ClapInfra.ClapJudgement;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Judgement
{
    public class JudgeRuleManager : ClapJudgeRuleManager<Community>
    {
        public class StageRuleContainer
        {
            public class RuleUnit
            {
                public ClapRoundClock Clock;
                public Action<Community, Community> Rule;
                
                public RuleUnit(ClapRoundClock clock, Action<Community, Community> rule)
                {
                    Clock = clock;
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
                _overrideRules.RemoveAll(o => o.Clock.IsDead);
                _modifiersBefore.RemoveAll(o => o.Clock.IsDead);
                _modifiersAfter.RemoveAll(o => o.Clock.IsDead);

                _overrideRules.ForEach(o => o.Clock.RoundPass());
                _modifiersBefore.ForEach(o => o.Clock.RoundPass());
                _modifiersAfter.ForEach(o => o.Clock.RoundPass());
            }
            public void Execute(Community player, Community enemy)
            {
                Action<Community, Community>? overrideRule = null;
                for (int i = _overrideRules.Count - 1; i >= 0; i--)
                {
                    if (_overrideRules[i].Clock.IsRinging)
                    {
                        overrideRule = _overrideRules[i].Rule;
                        break;
                    }
                }
                {
                    // BEFORE modifiers
                    foreach (var rule in _modifiersBefore)
                    {
                        if (rule.Clock.IsRinging)
                        {
                            rule.Rule(player, enemy);
                        }
                    }
                    // 核心规则
                    var core = overrideRule ?? _baseRule;
                    core(player, enemy);
                    // AFTER modifiers
                    foreach (var rule in _modifiersAfter)
                    {
                        if (rule.Clock.IsRinging)
                        {
                            rule.Rule(player, enemy);
                        }
                    }
                }
                ;
                Update();
            }
        }
        private readonly static SortedDictionary<JudgeStage.CEValue, StageRuleContainer> _defaultRuleContainers = new()
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
                new(UpdateCommunity)
            }
        };
        private static Action<Community, Community> _defaultRule;
        private int _notDefaultRounds = 0;
        static JudgeRuleManager()
        {
            _defaultRule = (a, b) =>
            {
                foreach (var stage in _defaultRuleContainers)
                {
                    stage.Value.Execute(a, b);
                }
            };
        }
        private readonly SortedDictionary<JudgeStage.CEValue, StageRuleContainer> _ruleContainers = new()
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
                new(UpdateCommunity)
            }
        };
        #region Default Rules（原有逻辑）
        private static void TakeEffects(EffectType.CEValue type, Community player, Community enemy)
        {
            foreach (var temp in player.BodyList)
            {
                temp.Get<Effect>().Execute(type, temp);
            }

            foreach (var temp in enemy.BodyList)
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
            var playerTemp = playerResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() && e.Clock.IsRinging).ToHashSet();
            var enemyTemp = enemyResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() && e.Clock.IsRinging).ToHashSet();

            playerResolutions.RemoveAll(playerTemp.Contains);
            enemyResolutions.RemoveAll(enemyTemp.Contains);

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
            playerResolutions = playerResolutions.OrderBy(a => a.Type).ToList();
            enemyResolutions = enemyResolutions.OrderBy(a => a.Type).ToList();
            int playerIndex = 0;
            int enemyIndex = 0;

            while (playerIndex < playerResolutions.Count && enemyIndex < enemyResolutions.Count)
            {
                var playerAttack = playerResolutions[playerIndex];
                var enemyAttack = enemyResolutions[enemyIndex];

                if (playerAttack.Type == AttackType.Instance.Real() || !playerAttack.Clock.IsRinging)
                {
                    playerIndex++;
                    continue;
                }

                if (enemyAttack.Type == AttackType.Instance.Real() || !enemyAttack.Clock.IsRinging)
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
            var playerTemp = playerResolutions.Where(e => e.Clock.IsRinging).ToList();
            var enemyTemp = enemyResolutions.Where(e => e.Clock.IsRinging).ToList();

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
            foreach (var temp in player.BodyList)
                temp.Update();

            foreach (var temp in enemy.BodyList)
                temp.Update();
        }
        private static void UpdateCommunity(Community player, Community enemy)
        {
            player.Update();
            enemy.Update();
        }
        #endregion
        public override void Judge(Community player, Community enemy)
        {
            if (_notDefaultRounds == 0)
            {
                _defaultRule(player, enemy);
            }
            _notDefaultRounds--;
            {
                foreach (var stage in _ruleContainers)
                {
                    stage.Value.Execute(player, enemy);
                }
            }
            ;
        }

        public void AddJudgeRule(Community source, List<Mutation> mutations)
        {
            mutations.ForEach(m =>
            {
                var originalRule = m.JudgeRule;

                m.JudgeRule = (player, enemy) =>
                {
                    if (source == player)
                    {
                        originalRule(player, enemy); 
                    }
                    else
                    {
                        originalRule(enemy, player); 
                    }
                };
            });
            foreach (var mutation in mutations)
            {
                StageRuleContainer.RuleUnit unit = new(mutation.Clock, mutation.JudgeRule);
                if (mutation.RuleType == RuleType.Override)
                {
                    _ruleContainers[mutation.Stage].AddOverride(unit);
                }
                else
                {
                    _ruleContainers[mutation.Stage].AddModifier(unit, mutation.ModifierOrder);
                }
                _notDefaultRounds = Math.Max(_notDefaultRounds, mutation.Clock.RemainingRounds + mutation.Clock.DelayRounds);
            }
        }
    }
}
