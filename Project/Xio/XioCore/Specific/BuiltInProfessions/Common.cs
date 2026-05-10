using XioCore.Infra.DSL;
using XioCore.Infra.Models.Components;
using XioCore.Infra.Models.Components.Resolutions;
using XioCore.Infra.Models.Core;
using XioCore.Infra.Models.Entities;
using XioCore.Infra.Profession;

namespace XioCore.Specific.BuiltInProfessions
{
    using DSL = XioCore.Infra.DSL.DSLforSkillLogic;
    using Pen = System.Func<XioCore.Infra.DSL.DSLforSkillLogic.SourceFile, XioCore.Infra.DSL.DSLforSkillLogic.SourceFile>;
    public partial class Common : MainProfession
    {
        private static int RNeed(ISkillContext sc)
        {
            return (int)Math.Pow(3, sc.Rank);
        }
        private static int LBase(ISkillContext sc)
        {
            return sc.Rank * Level.CycleLength;
        }

        private static bool CheckResource(ISkillContext sc, ResourceType.CEValue resourceType, int baseline = 1)
        {
            return sc.Self.Get<Resource>().Check(baseline * RNeed(sc), resourceType);
        }
        private static bool CheckResource(ISkillContext sc, int baseline = 1)
        {
            return sc.Self.Get<Resource>().Check(baseline * RNeed(sc), ResourceType.Instance.Xio());
        }
        private static bool CheckRank(ISkillContext sc, int internalLevel)
        {
            var level = sc.Self.Get<Level>();
            return level.Rank * Level.CycleLength + level.InternalLevel
                >= LBase(sc) + internalLevel;
        }
        //1自割
        #region
        private bool ZgCheck(ISkillContext sc) => true;
        private IDSLSourceFile Zg(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree((Body body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Zige(),
                        Execute = (Body target) =>
                        {
                            target.Get<Level>().KilledTimes++;
                        }
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                });
            return DSL.Create(pen);
        }
        #endregion
        //1xio
        #region
        private bool XCheck(ISkillContext sc) => true;
        private IDSLSourceFile X(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteResource(RNeed(sc));
            return DSL.Create(pen);
        }
        #endregion
        //1钻三五雷
        #region
        private bool ZswlCheck(ISkillContext sc)
        {
            return CheckRank(sc, 1) && CheckResource(sc, baseline: 3);
        }
        private IDSLSourceFile Zswl(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3 * RNeed(sc))
                .WriteAttack(LBase(sc) + 3.5f, DefenseType.Instance.Heng(), factor: 3 * RNeed(sc));
            return DSL.Create(pen);
        }
        #endregion
        //1升级
        #region
        private bool ZyCheck(ISkillContext sc)
        {
            return CheckRank(sc, 1) && CheckResource(sc, baseline: 3);
        }
        private IDSLSourceFile Zy(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3 * RNeed(sc))
                .WriteShengji(1);
            return DSL.Create(pen);
        }
        private bool JhCheck(ISkillContext sc)
        {
            return CheckRank(sc, 1) && CheckResource(sc, baseline: 6);
        }
        private IDSLSourceFile Jh(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(6 * RNeed(sc))
                .WriteShengji(3);
            return DSL.Create(pen);
        }
        private bool CsCheck(ISkillContext sc)
        {
            return CheckRank(sc, 1) && CheckResource(sc, baseline: 9);
        }
        private IDSLSourceFile Cs(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(9 * RNeed(sc))
                .WriteShengji(6);
            return DSL.Create(pen);
        }
        #endregion
        //1三劈
        #region
        private bool XpCheck(ISkillContext sc) => true;
        private IDSLSourceFile Xp(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Xie());
            return DSL.Create(pen);
        }
        private bool HpCheck(ISkillContext sc) => true;
        private IDSLSourceFile Hp(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Heng());
            return DSL.Create(pen);
        }
        private bool SpCheck(ISkillContext sc) => true;
        private IDSLSourceFile Sp(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        #endregion
        //1刀子
        #region
        private bool DzCheck(ISkillContext sc)
        {
            return CheckRank(sc, 1) && CheckResource(sc);
        }
        private IDSLSourceFile Dz(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 1, DefenseType.Instance.Xie());
            return DSL.Create(pen);
        }
        #endregion
        //2天马
        #region
        private bool TmCheck(ISkillContext sc)
        {
            return CheckRank(sc, 2) && CheckResource(sc);
        }
        private IDSLSourceFile Tm(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 2, DefenseType.Instance.Xie());
            return DSL.Create(pen);
        }
        #endregion
        //3冰盾冰锤
        #region
        private bool BdCheck(ISkillContext sc)
        {
            return CheckRank(sc, 3);
        }
        private IDSLSourceFile Bd(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteResource(RNeed(sc), ResourceType.Instance.IceShield())
                .WriteDefense(DefenseType.Instance.Common(), LBase(sc) + 3);
            return DSL.Create(pen);
        }
        private bool BcCheck(ISkillContext sc)
        {
            return CheckRank(sc, 3) && CheckResource(sc, ResourceType.Instance.IceShield(), baseline: 3);
        }
        private IDSLSourceFile Bc(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3 * RNeed(sc), ResourceType.Instance.IceShield())
                .WriteAttack(LBase(sc) + 3.5f, DefenseType.Instance.Heng());
            return DSL.Create(pen);
        }
        #endregion
        //4小小
        #region
        private bool XxCheck(ISkillContext sc)
        {
            return CheckRank(sc, 4);
        }
        private IDSLSourceFile Xx(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree((Body body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Xiaoxiao(),
                        Execute = _ => { }
                    };
                    resolution.Execute = (Body target) =>
                    {
                        foreach (var resolution in target.Get<TurnContext>().Get<UniversalResolution>())
                        {
                            if (resolution.SkillType == SkillType.Instance.Resource()
                            && resolution.SkillType == SkillType.Instance.Shengji())
                            {
                                for (int i = 0; i < 10 * RNeed(sc); ++i)
                                {
                                    body.Get<TurnContext>().Get<UniversalResolution>().Add(resolution);
                                }
                            }
                        }
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                });
            return DSL.Create(pen);
        }
        #endregion
        //5-7三龙
        #region
        private bool SlCheck(ISkillContext sc)
        {
            return CheckRank(sc, 5) && CheckResource(sc);
        }
        private IDSLSourceFile Sl(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 5, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        private bool JlCheck(ISkillContext sc)
        {
            return CheckRank(sc, 6) && CheckResource(sc);
        }
        private IDSLSourceFile Jl(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 6, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        private bool KlCheck(ISkillContext sc)
        {
            return CheckRank(sc, 7) && CheckResource(sc);
        }
        private IDSLSourceFile Kl(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 7, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        #endregion
        //8弹
        #region
        private bool TCheck(ISkillContext sc)
        {
            return CheckRank(sc, 8);
        }
        private IDSLSourceFile T(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree((Body body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Tan(),
                        Power = LBase(sc) + 8,
                        Execute = _ => { }
                    };
                    resolution.Execute = (Body target) =>
                    {
                        var list = target.Get<TurnContext>().Get<UniversalResolution>();
                        int n = list.Count;
                        for (int j = n - 1; j >= 0; --j)
                        {
                            var r = list[j];
                            if (r.SkillType == SkillType.Instance.Resource()
                            && r.SkillType == SkillType.Instance.Shengji()
                            && r.SkillType == SkillType.Instance.Zige())
                            {
                                for (int i = 0; i < RNeed(sc); ++i)
                                {
                                    target.Get<TurnContext>().Get<UniversalResolution>().Add(r);
                                }
                            }
                            if (r.SkillType == SkillType.Instance.Attack())
                            {
                                if (!r.IsLyt)
                                {
                                    for (int i = 0; i < RNeed(sc); ++i)
                                    {
                                        body.Get<TurnContext>().Get<UniversalResolution>().Add(r);
                                    }
                                    target.Get<TurnContext>().Get<UniversalResolution>().Remove(r);
                                }
                                else
                                {
                                    if (resolution.Power - r.Power >= Level.CycleLength * 2 + 8 - 22)
                                    {
                                        for (int i = 0; i < RNeed(sc); ++i)
                                        {
                                            body.Get<TurnContext>().Get<UniversalResolution>().Add(r);
                                        }
                                        target.Get<TurnContext>().Get<UniversalResolution>().Remove(r);
                                    }
                                    else if (resolution.Power - r.Power >= Level.CycleLength + 8 - 22)
                                    {
                                        target.Get<TurnContext>().Get<UniversalResolution>().Remove(r);
                                    }
                                }
                            }
                            if (r.SkillType == SkillType.Instance.Taichi())
                            {
                                if (resolution.Power - r.Power >= Level.CycleLength + 8 - 16)
                                {
                                    for (int i = 0; i < RNeed(sc); ++i)
                                    {
                                        body.Get<TurnContext>().Get<UniversalResolution>().Add(r);
                                    }
                                }
                            }
                        }
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                });
            return DSL.Create(pen);
        }
        #endregion
        //9影
        #region
        private bool YCheck(ISkillContext sc)
        {
            return CheckRank(sc, 9);
        }
        private IDSLSourceFile Y(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Ying(), LBase(sc) + 22);
            return DSL.Create(pen);
        }
        #endregion
        //10光 影光冲刺
        #region
        private bool GCheck(ISkillContext sc)
        {
            return CheckRank(sc, 10) && CheckResource(sc);
        }
        private IDSLSourceFile G(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteResource(RNeed(sc), ResourceType.Instance.Light())
                .WriteAttack(LBase(sc) + 10, DefenseType.Instance.Heng());
            return DSL.Create(pen);
        }
        private bool YgccCheck(ISkillContext sc)
        {
            return CheckRank(sc, 10) && CheckResource(sc, ResourceType.Instance.Light());
        }
        private IDSLSourceFile Ygcc(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc), ResourceType.Instance.Light())
                .WriteFree((Body body) =>
                {
                    var resolution = new UniversalResolution()
                    {
                        SkillType = SkillType.Instance.Attack(),
                        Power = LBase(sc) + 10.5f,
                        Execute = _ => { }
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
                            if (defense.DefenseType == DefenseType.Instance.Heng())
                            {
                                return;
                            }
                            if (defense.DefenseType == DefenseType.Instance.Ying()
                            && temp - defense.Power >= 10.5f - 22f)
                            {
                                continue;
                            }
                            temp = defense.Work(temp);
                            if (temp <= 0)
                            {
                                return;
                            }
                        }
                        target.Get<Level>().KilledTimes++;
                    };
                    body.Get<TurnContext>().WriteResolution(resolution);
                });
            return DSL.Create(pen);
        }
        #endregion
        //11-16 太极系列 
        #region
        private bool TkCheck(ISkillContext sc)
        {
            return CheckRank(sc, 11);
        }
        private IDSLSourceFile Tk(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Common(), LBase(sc) + 11)
                .WriteSuck(LBase(sc) + 11, RNeed(sc));
            return DSL.Create(pen);
        }
        private bool XkCheck(ISkillContext sc)
        {
            return CheckRank(sc, 12);
        }
        private IDSLSourceFile Xk(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Common(), LBase(sc) + 12)
                .WriteSuck(LBase(sc) + 12, RNeed(sc));
            return DSL.Create(pen);
        }
        private bool YkCheck(ISkillContext sc)
        {
            return CheckRank(sc, 13);
        }
        private IDSLSourceFile Yk(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Common(), LBase(sc) + 13)
                .WriteSuck(LBase(sc) + 13, RNeed(sc));
            return DSL.Create(pen);
        }
        private bool JkCheck(ISkillContext sc)
        {
            return CheckRank(sc, 14);
        }
        private IDSLSourceFile Jk(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Common(), LBase(sc) + 14)
                .WriteSuck(LBase(sc) + 14, RNeed(sc));
            return DSL.Create(pen);
        }
        private bool GpCheck(ISkillContext sc)
        {
            return CheckRank(sc, 15);
        }
        private IDSLSourceFile Gp(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Common(), LBase(sc) + 15)
                .WriteSuck(LBase(sc) + 15, RNeed(sc));
            return DSL.Create(pen);
        }
        private bool TjCheck(ISkillContext sc)
        {
            return CheckRank(sc, 16);
        }
        private IDSLSourceFile Tj(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(DefenseType.Instance.Common(), LBase(sc) + 16)
                .WriteSuck(LBase(sc) + 16, RNeed(sc));
            return DSL.Create(pen);
        }
        #endregion
        //17-21 三魔三鱼头
        #region
        private bool XmCheck(ISkillContext sc)
        {
            return CheckRank(sc, 17) && CheckResource(sc);
        }
        private IDSLSourceFile Xm(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 17, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        private bool DmCheck(ISkillContext sc)
        {
            return CheckRank(sc, 18) && CheckResource(sc);
        }
        private IDSLSourceFile Dm(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 18, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        private bool LmCheck(ISkillContext sc)
        {
            return CheckRank(sc, 19) && CheckResource(sc);
        }
        private IDSLSourceFile Lm(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 19, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }

        private bool XytCheck(ISkillContext sc)
        {
            return CheckRank(sc, 20) && CheckResource(sc);
        }
        private IDSLSourceFile Xyt(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 20, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        private bool DytCheck(ISkillContext sc)
        {
            return CheckRank(sc, 21) && CheckResource(sc);
        }
        private IDSLSourceFile Dyt(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 21, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        private bool LytCheck(ISkillContext sc)
        {
            return CheckRank(sc, 22) && CheckResource(sc);
        }
        private IDSLSourceFile Lyt(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(RNeed(sc))
                .WriteAttack(LBase(sc) + 22, DefenseType.Instance.Shu());
            return DSL.Create(pen);
        }
        #endregion
    }
}
