using XioCore.Infra.Models.Components;
using XioCore.Infra.Models.Components.Resolutions;
using XioCore.Infra.Models.Core;
using XioCore.Infra.Models.Entities;
using XioCore.Infra.Models.Judgement;
using XioCore.Infra.Models.Judgement.Core;

namespace XioCore.Infra.DSL
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public static class DSLforSkillLogic
    {
        public class SourceFile : IDSLSourceFile
        {
            private Action<Body> _sentence = _ => { };
            public Intent Compile(Judger? judger = null)
            {
                return new() { Execute = _sentence };
            }
            public SourceFile WriteFree(Action<Body> action)
            {
                _sentence += action;
                return this;
            }
            public SourceFile UseResource(int need)
            {
                return WriteFree((Body body) =>
                {
                    body.Get<Resource>().Use(need, ResourceType.Instance.Xio());
                });
            }
            public SourceFile WriteShengji(int addition)
            {
                _sentence += (Body body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Shengji(),
                        Execute = _ => { }
                    };
                    resolution.Execute = (Body target) =>
                    {
                        target.Get<Level>().Upgrade(addition);
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                };

                return this;
            }
            public SourceFile UseResource(int need, ResourceType.CEValue resourceType)
            {
                return WriteFree((Body body) =>
                {
                    body.Get<Resource>().Use(need, resourceType);
                });
            }
            public SourceFile WriteAttack(float power, DefenseType.CEValue restrainType,
                int factor = 1, bool isLyt = false)
            {
                _sentence += (Body body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Attack(),
                        Power = power,
                        Execute = _ => { },
                        IsLyt = isLyt
                    };
                    resolution.Execute = (target) =>
                    {
                        if (resolution.Power <= 0)
                        {
                            return;
                        }
                        var temp = resolution.Power;
                        foreach (var defense in target.Get<Defense>().Defenses)
                        {
                            if (defense.DefenseType == restrainType)
                            {
                                return;
                            }
                            temp = defense.Work(temp);
                            if (temp <= 0)
                            {
                                return;
                            }
                        }
                        target.Get<Level>().KilledTimes += factor;
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                };
                return this;
            }
            public SourceFile WriteDefense(DefenseType.CEValue defenseType, float power = 0)
            {
                var defense = new Reduction(defenseType, power);
                _sentence += (body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Defense(),
                        Execute = _ => { }
                    };
                    resolution.Execute = (target) =>
                    {
                        target.Get<Defense>().Add(defense);
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                };
                return this;
            }
            public SourceFile WriteResource(int power)
            {
                _sentence += (body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Resource(),
                        Execute = _ => { }
                    };
                    resolution.Execute = (target) =>
                    {
                        target.Get<Resource>().Gain(power, ResourceType.Instance.Xio());
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                };
                return this;
            }
            public SourceFile WriteResource(int power, ResourceType.CEValue resourceType)
            {
                _sentence += (body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Resource(),

                        Execute = _ => { }
                    };
                    resolution.Execute = (target) =>
                    {
                        target.Get<Resource>().Gain(power, resourceType);
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                };
                return this;
            }
            public SourceFile WriteSuck(int power, int need)
            {
                _sentence += (Body body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Shengji(),
                        Power = power,
                        Execute = _ => { }
                    };
                    resolution.Execute = (Body target) =>
                    {
                        var rlist = target.Get<TurnContext>().Get<UniversalResolution>();
                        if (target.Get<Defense>().Defenses.Find(d => d.DefenseType == DefenseType.Instance.Xie()) != null)
                        {
                            body.Get<Level>().KilledTimes++;
                            return;
                        }
                        List<SkillType.CEValue> wl = new()
                        {
                            SkillType.Instance.Attack(),
                            SkillType.Instance.Xiaoxiao()
                        };
                        if (rlist.Find(r => wl.Contains(r.SkillType)) != null)
                        {
                            return;
                        }
                        var t = rlist.Find(r => r.SkillType == SkillType.Instance.Taichi());
                        if (t != null)
                        {
                            if (t.Power < resolution.Power)
                            {
                                target.Get<Level>().KilledTimes++;
                            }
                            else if (t.Power > resolution.Power)
                            {
                                body.Get<Level>().KilledTimes++;
                            }
                            return;
                        }
                        if (target.Get<Resource>().Check(need, ResourceType.Instance.Xio()))
                        {
                            target.Get<Resource>().Use(need, ResourceType.Instance.Xio());
                            body.Get<Resource>().Gain(need, ResourceType.Instance.Xio());
                        }
                        else
                        {
                            target.Get<Level>().KilledTimes++;
                        }
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                };
                return this;
            }
        }
        public static SourceFile Create(Pen pen)
        {
            return pen(new());
        }
    }
}
