using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Common : MainProfession
    {
        private static IReadOnlySet<string> ProfessionSkillNames => ProfessionRegistry.MainProfessionSkillNames;

        private bool IronCheck(ISkillContext sc) => true;
        [HasResource]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile Iron(ISkillContext sc)
        {

            Pen pen = sf => sf.WriteResource(1, ResourceType.Instance.Iron());
            return DSL.Create(sc.Self, pen);
        }

        private bool StickCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        [HasAttack(1)]
        [Labels(Impression.Conservative, Strength.Useless)]
        private IDSLSourceFile Stick(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteAttack(1, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool DrillCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1.5f);
        }
        [HasAttack(3)]
        [Labels(Impression.Conservative, Strength.Useless)]
        private IDSLSourceFile Drill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Instance.Iron())
                .WriteAttack(3, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool SlashCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2.5f);
        }
        [HasAttack(1)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile Slash(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2.5f, ResourceType.Instance.Iron())
                .WriteAttack(5, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool ShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.Param * 0.5f);
        }
        [HasDefense]
        [Labels(Impression.Conservative, Strength.Useless)]
        private IDSLSourceFile Shield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(2 + sc.Param, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool ThornShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1 + sc.Param * 0.5f);
        }
        [HasDefense]
        [Labels(Impression.Robust, Strength.Useless)]
        private IDSLSourceFile ThornShield(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param * 0.5f, ResourceType.Instance.Iron())
                .WriteDefense(4 + sc.Param, new ThornReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool RecoveryCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1 + sc.Param);
        }
        [HasRecovery]
        [Labels(Impression.Conservative, Strength.Useless)]
        private IDSLSourceFile Recovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1 + sc.Param, ResourceType.Instance.Iron())
                .WriteRecovery(2 + 2 * sc.Param);
            return DSL.Create(sc.Self, pen);
        }

        private bool SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile Space(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Space());
            return DSL.Create(sc.Self, pen);
        }

        private bool TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Useless)]
        private IDSLSourceFile Time(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Time());
            return DSL.Create(sc.Self, pen);
        }

        private bool TearCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 1f);
        }
        [HasAttack(8)]
        [Labels(Impression.Aggressive, Strength.Strong)]
        private IDSLSourceFile Tear(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Space())
                .WriteAttack(8, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
        private bool ReflectCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 2f);
        }
        [HasDefense]
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Super)]
        private IDSLSourceFile Reflect(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Space())
                .RegistCallbackOnJudge(
                    new()
                    {
                        new ModifierCallback(
                            ReflectRule.EffectSwaping_Modifier_After,
                            JudgeStage.Instance.OnEffectSwaping(),
                            ModifierOrder.After,
                            new()),
                        new ModifierCallback(
                            ReflectRule.AttackSwaping_Modifier_After,
                            JudgeStage.Instance.OnAttackSwaping(),
                            ModifierOrder.After,
                            new())
                    });
            return DSL.Create(sc.Self, pen);
        }

        private bool WarlockCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1f);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile Warlock(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Warlock()));
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                }, false);
            return DSL.Create(sc.Self, pen);
        }

        private bool CannonCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 4);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile Cannon(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Cannon()));
            Pen pen = sf => sf
                .UseResource(4, ResourceType.Instance.Iron())
                .WriteDefense(3, new CommonReduction())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                }, false);
            return DSL.Create(sc.Self, pen);
        }

        private bool DriverCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile Driver(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Driver()));
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                }, false);
            return DSL.Create(sc.Self, pen);
        }

        private bool BloodSigilCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 7);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile BloodSigil(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new BloodSigil()));
            Pen pen = sf => sf
                .UseResource(7, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                    List<string> addition = new()
                    {
                        nameof(Stick).ToLower(),
                        nameof(Drill).ToLower(),
                        nameof(Slash).ToLower(),
                        nameof(Tear).ToLower()
                    };
                    addition.ForEach(a => source.Focus.Get<Skill>().RemoveSkill(nameof(Common), a));
                    source.Focus.Get<Health>().GainMHP(3);
                    source.Focus.Get<Health>().GainHP(3);
                }, false);
            return DSL.Create(sc.Self, pen);
        }
        private bool LancerCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [IsProfessionSkill]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile Lancer(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Lancer()));
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    ExcludeAllProfessions(source);
                }, false);
            return DSL.Create(sc.Self, pen);
        }
        public static void ExcludeAllProfessions(Community source)
        {
            foreach (var name in ProfessionSkillNames)
            {
                source.Focus.Get<Skill>().RemoveSkill(nameof(Common), name);
            }
        }
    }
    public static class ReflectRule
    {
        public static void EffectSwaping_Modifier_After(Community player, Community enemy)
        {
            var playerResolutions = player.Focus.Get<TurnContext>().Get<EffectResolution>();

            var reflect = playerResolutions.Where(e => e.TargetType == EffectTargetType.Instance.Enemy() || e.Clock.IsRinging);

            playerResolutions.RemoveAll(reflect.Contains);

            foreach (var e in reflect)
            {
                e.Clock.DelayRounds = 1;
            }
            playerResolutions.AddRange(reflect);
        }
        public static void AttackSwaping_Modifier_After(Community player, Community enemy)
        {
            var tc = player.Focus.Get<TurnContext>();
            var playerResolutions = tc.Get<AttackResolution>();

            var reflect = playerResolutions.Where(a => a.Clock.IsRinging);

            playerResolutions.RemoveAll(a => reflect.Contains(a));

            foreach (var a in reflect)
            {
                a.Clock.DelayRounds = 1;
                a.Source = player;
            }

            foreach (var res in reflect)
            {
                tc.WriteResolution(res);
            }
        }
    }
}