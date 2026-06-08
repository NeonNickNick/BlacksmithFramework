using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
namespace BlacksmithCore.Infra.DSL
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public static class DSLforSkillLogic
    {
        public class SourceFile : IDSLSourceFile
        {
            protected enum SentenceType
            {
                Attack,
                Defense,
                Resource,
                Effect,
                Recovery,
                Free
            }
            protected enum StructureType
            {
                Main,
                Rhetoric
            }
            protected class Sentence
            {
                public Action<Community> Structure { get; }
                public SentenceType SentenceType { get; }
                public StructureType StructureType { get; }
                public Sentence? BindSentence { get; }
                public bool CanMove { get; } = true;
                public Sentence(Action<Community> structure, SentenceType sentenceType, StructureType structureType, Sentence? bindSentence = null, bool canMove = true)
                {
                    Structure = structure;
                    SentenceType = sentenceType;
                    StructureType = structureType;
                    BindSentence = bindSentence;
                    CanMove = canMove;
                }
            }
            public bool IsPassive { get; set; } = false;

            protected Community _owner;
            protected List<Sentence> _sentences = new();
            protected Stack<Sentence> _rhetoricCache = new();
            protected List<List<ICallbackOnJudge>> _mutationsOnCompile = new();
            protected SourceFile(SourceFile origin)
            {
                _owner = origin._owner;
                _sentences = origin._sentences;
                _rhetoricCache = origin._rhetoricCache;
                _mutationsOnCompile = origin._mutationsOnCompile;
            }
            public SourceFile(Community owner)
            {
                _owner = owner;
            }
            public void Move(Community newOwner)
            {
                _owner = newOwner;
                _sentences.RemoveAll(s => !s.CanMove);
            }
            public Intent Compile(Judger? judger = null)
            {
                List<Sentence> sentences = new(_sentences);
                int n = _rhetoricCache.Count;
                for (int i = 0; i < n; ++i)
                {
                    var rhetoric = _rhetoricCache.Pop();
                    int index = sentences.IndexOf(rhetoric.BindSentence!) + 1;
                    if (index > 0)
                    {
                        sentences.Insert(index, new(rhetoric.Structure, rhetoric.SentenceType, StructureType.Rhetoric));
                    }
                }
                Action<Community> result = (a) => { };
                if (judger != null)
                {
                    foreach (var temp in _mutationsOnCompile)
                    {
                        judger.JudgeRuleManager.AddJudgeRule(_owner, temp);
                    }
                }
                foreach (var sentence in sentences)
                {
                    result += sentence.Structure;
                }
                return new Intent() { Execute = result };
            }
            public SourceFile WriteFree(Action<Community> action, bool canMove)
            {
                _sentences.Add(new(action, SentenceType.Free, StructureType.Main, canMove: canMove));
                return this;
            }
            public AttackFile WriteAttack(
                float power,
                AttackType.CEValue attackType,
                float APFactor = 1,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new AttackResolution
                    {
                        Source = source,
                        Clock = new(delayRounds: delayRounds),
                        Type = attackType,
                        Power = power
                    };
                    resolution.Execute = (target) =>
                    {
                        Body main = target.Focus;
                        if (resolution.Power <= 0f)
                        {
                            return;
                        }
                        bool ifHitArmor = false;
                        if (resolution.Type != AttackType.Instance.Real())
                        {
                            var defenses = main.Get<Defense>().Get();
                            var APList = new List<DefenseType.CEValue>()
                            {
                                DefenseType.Instance.ThornReduction(),
                                DefenseType.Instance.CommonReduction(),
                                DefenseType.Instance.StoneShell(),
                                DefenseType.Instance.RealArmor(),
                                DefenseType.Instance.CommonArmor()
                            };
                            var armorList = new List<DefenseType.CEValue>()
                            {
                                DefenseType.Instance.StoneShell(),
                                DefenseType.Instance.RealArmor(),
                                DefenseType.Instance.CommonArmor()
                            };

                            foreach (var temp in defenses)
                            {
                                if (!ifHitArmor && armorList.Contains(temp.Type))
                                {
                                    ifHitArmor = true;
                                    resolution.RunStage(AttackStage.OnHitArmorFirstTime, main);
                                }
                                if (APList.Contains(temp.Type))
                                {
                                    resolution.Power *= APFactor;
                                }
                                var res = temp.Work(resolution.Source.Focus, main, (int)resolution.Power, resolution.Type);
                                resolution.Power = res.Item1;
                                resolution.TotalDamage += res.Item2;
                                if (APList.Contains(temp.Type))
                                {
                                    resolution.Power = MathF.Ceiling(resolution.Power / APFactor);
                                }
                                if (resolution.Power <= 0f)
                                {
                                    resolution.RunStage(AttackStage.OnEnd, main);
                                    return;
                                }
                            }
                        }
                        if (!ifHitArmor)
                        {
                            resolution.RunStage(AttackStage.OnHitArmorFirstTime, main);
                        }
                        resolution.RunStage(AttackStage.OnHitBody, main);
                        main.Get<Health>().LoseHP((int)resolution.Power);
                        resolution.TotalDamage += (int)resolution.Power;
                        resolution.RunStage(AttackStage.OnEnd, main);
                    };
                    resolution.Source.Focus.Get<TurnContext>().WriteResolution(resolution);
                }, SentenceType.Attack, StructureType.Main));
                return new(this);
            }

            public RecoveryFile WriteRecovery(int power)
            {
                _sentences.Add(new((source) =>
                {
                    source.Focus.Get<Health>().GainHP(power);
                }, SentenceType.Recovery, StructureType.Main));
                return new(this);
            }
            public DefenseFile WriteDefense(
                float power,
                DefenseBase defense,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new DefenseResolution()
                    {
                        Clock = new(delayRounds: delayRounds),
                        Defense = defense,
                        Power = power
                    };
                    resolution.Execute = (target) =>
                    {
                        defense.Power = (int)resolution.Power;
                        defense.Owner = source;
                        target.Focus.Get<Defense>().Add(resolution.Defense);
                    };
                    source.Focus.Get<TurnContext>().WriteResolution(resolution);
                }, SentenceType.Defense, StructureType.Main));
                return new(this);
            }
            public ResourceFile WriteResource(
                float power,
                ResourceType.CEValue type,
                int delayRounds = 0
            )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new ResourceResolution()
                    {
                        Clock = new(delayRounds: delayRounds),
                        Power = power,
                        Type = type
                    };
                    resolution.Execute = (target) =>
                    {
                        target.Focus.Get<Resource>().Gain(resolution.Type, resolution.Power);
                    };
                    source.Focus.Get<TurnContext>().WriteResolution(resolution);
                }, SentenceType.Resource, StructureType.Main));
                return new(this);
            }
            public EffectFile WriteEffect(
                EffectType.CEValue type,
                EffectTargetType.CEValue targetType,
                float power,
                int duration,
                Action<Community, Body, EffectEntity> effectAction,
                int delayRounds = 0
                )
            {
                _sentences.Add(new((source) =>
                {
                    var resolution = new EffectResolution(new(delayRounds: delayRounds), type, targetType, power);
                    resolution.Execute = (target) =>
                    {
                        Body main = target.Focus;
                        var entity = new EffectEntity(resolution.Type, resolution.Power, new(remainingRounds: duration));
                        entity.Execute = (body) => effectAction(source, body, entity);
                        main.Get<Effect>().Add(entity);
                        resolution.RunStage(EffectStage.OnSuccessfullyAdded, source, main);
                    };
                    source.Focus.Get<TurnContext>().WriteResolution(resolution);
                }, SentenceType.Effect, StructureType.Main));
                return new(this);
            }
            public SourceFile UseResource(float need, ResourceType.CEValue type, bool ifCommonOnly = false)
            {
                return WriteFree(source => source.Focus.Get<Resource>().Use(type, need, ifCommonOnly), false);
            }
            public SourceFile LoseHP(int loss)
            {
                return WriteFree(source => source.Focus.Get<Health>().LoseHP(loss), false);
            }
            public SourceFile LoseMHP(int loss)
            {
                return WriteFree(source => source.Focus.Get<Health>().LoseMHP(loss), false);
            }
            public SourceFile RegistCallbackOnJudge(
                List<ICallbackOnJudge> mutations)
            {
                _mutationsOnCompile.Add(mutations);
                return this;
            }

        }
        public class DefenseFile : SourceFile
        {
            public DefenseFile(SourceFile self) : base(self)
            {
            }
        }
        public class RecoveryFile : SourceFile
        {
            public RecoveryFile(SourceFile self) : base(self)
            {
            }
        }
        public class AttackFile : SourceFile
        {
            public AttackFile WithFree(AttackStage stage, Action<Community, Body, AttackResolution> action)
            {
                _rhetoricCache.Push(new((source) =>
                {
                    var list = source.Focus.Get<TurnContext>().Get<AttackResolution>();
                    if (list.Count == 0)
                    {
                        return;
                    }
                    var last = list[^1];
                    last.AddStage(stage, action);
                }, SentenceType.Attack, StructureType.Rhetoric, _sentences[^1]));
                return this;
            }
            public AttackFile WithBloodSuck(float percent)
            {
                var suck = (Community source, Body target, AttackResolution resolution) =>
                {
                    source.Focus.Get<Health>().GainHP((int)MathF.Ceiling(resolution.Power * percent));
                };
                return WithFree(AttackStage.OnEnd, suck);
            }
            public AttackFile WithInterupt()
            {
                var interuptList = new List<ResourceType.CEValue>()
                {
                    ResourceType.Instance.Iron(),
                    ResourceType.Instance.Gold_Iron(),
                    ResourceType.Instance.Magic()
                };
                var interupt = (Community source, Body target, AttackResolution resolution) =>
                {
                    target.Get<TurnContext>().Get<ResourceResolution>().RemoveAll(r => interuptList.Contains(r.Type));
                };
                return WithFree(AttackStage.OnHitArmorFirstTime, interupt);
            }
            public AttackFile(SourceFile self) : base(self)
            {
            }
        }
        public class ResourceFile : SourceFile
        {
            public ResourceFile(SourceFile self) : base(self)
            {
            }
        }
        public class EffectFile : SourceFile
        {
            public EffectFile(SourceFile self) : base(self)
            {
            }
        }

        public static SourceFile Create(Community source, Pen Pen)
        {
            var sourceFile = new SourceFile(source);
            return Pen(sourceFile);
        }
    }
}
