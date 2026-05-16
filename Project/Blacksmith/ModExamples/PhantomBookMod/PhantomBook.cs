using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Attributes.MarkOnly;
using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
using BlacksmithCore.Infra.Profession;
using ClapInfra.ClapModels.Components;
using ModExamples.PhantomBookMod.Defense;

namespace ModExamples.PhantomBookMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    [IsExperimental]
    public partial class PhantomBook : MainProfession
    {
        private static HashSet<string> _nightmareExclusive = new()
        {

        };
        public PhantomBook()
        {
            foreach (var name in _nightmareExclusive)
            {
                AvailableSkillNames.Remove(name);
            }
        }
        private bool FantasiaCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private IDSLSourceFile Fantasia(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Dream());
            return DSL.Create(sc.Self, pen);
        }

        private bool AssociationCheck(ISkillContext sc)
        {
            string expectedSkill = sc.StringParam;
            var swapInstance = sc.SudoOperations.DeepCopy();
            swapInstance.Swap();
            var fakeSelf = sc.SudoOperations.IsPlayer(sc.Self) ? swapInstance.Player : swapInstance.Enemy;
            var fakeSkill = fakeSelf.Focus.Get<Skill>();
            var fsc = new DefaultSkillContext(swapInstance, expectedSkill, fakeSelf, sc.Param, sc.StringParam);
            if (sc.SudoOperations.EquipmentSkillNames.Contains(expectedSkill) ||
                sc.SudoOperations.ProfessionSkillNames.Contains(expectedSkill) ||
                expectedSkill == $"{nameof(Association).ToLower()}" || 
                fakeSkill.TryDeclare(fsc.SkillName, fsc) != SkillDeclareResult.Success)
            {
                return false;
            }
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        private IDSLSourceFile Association(ISkillContext sc)
        {
            string expectedSkill = sc.StringParam;
            var swapInstance = sc.SudoOperations.DeepCopy();
            swapInstance.Swap();
            var fakeSelf = sc.SudoOperations.IsPlayer(sc.Self) ? swapInstance.Player : swapInstance.Enemy;
            var fakeSkill = fakeSelf.Focus.Get<Skill>();
            var fsc = new DefaultSkillContext(swapInstance, expectedSkill, fakeSelf, sc.Param, sc.StringParam);
            var stolenSF = fakeSkill.Declare(fsc.SkillName, fsc);
            stolenSF.Move(sc.Self);
            return ((DSL.SourceFile)stolenSF)
                .UseResource(2f, ResourceType.Instance.Dream());
        }
        private bool HallucinateCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        private IDSLSourceFile Hallucinate(ISkillContext sc)
        {
            Pen pen = sf => sf
               .WriteEffect(EffectType.Instance.AfterResolutionWritten(), EffectTargetType.Instance.Enemy(), 0, 1,
               (Community source, Body main, EffectEntity effectEntity) =>
               {
                   var tc = main.Get<TurnContext>();
                   tc.Get<AttackResolution>().ForEach(a => a.DelayRounds++);
                   tc.AddPreprocess<AttackResolution>(a => a.DelayRounds++);
               });
            return DSL.Create(sc.Self, pen);
        }
        private bool AwakeningCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        [IsHighCost]
        private IDSLSourceFile Awakening(ISkillContext sc)
        {
            var sandBoxInstance = sc.SudoOperations.DeepCopy(preRounds: 2);
            Body copiedBody = sc.SudoOperations.IsPlayer(sc.Self) ? sandBoxInstance.Player.Focus : sandBoxInstance.Enemy.Focus;
            var resource = copiedBody.Get<Resource>();
            float m = MathF.Min(2f, resource.QueryAll(ResourceType.Instance.Dream()));
            resource.Use(ResourceType.Instance.Dream(), m);
            sc.Self.ReplaceDelayed(copiedBody);
            return DSL.Create(sc.Self, _ => _);
        }
        private bool IllusionCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 1f);
        }
        private IDSLSourceFile Illusion(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .WriteRecovery(5);
            return DSL.Create(sc.Self, pen);
        }
        private bool NightmareCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 3f)
                && sc.Self.Focus.Get<Health>().HP > 1;
        }
        [IsEquipmentSkill]
        private IDSLSourceFile Nightmare(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .LoseHP(5)
                .WriteDefense(0f, new PhysicalImmunity())
                .WriteDefense(0f, new NightmareArmor(() =>
                {
                    foreach (var name in _nightmareExclusive)
                    {
                        sc.Self.Focus.Get<Skill>().RemoveSkill(nameof(PhantomBook), name);
                    }
                    sc.Self.Focus.Get<Skill>().AddSkill(nameof(PhantomBook), nameof(Nightmare).ToLower());
                }))
                .WriteFree(source =>
                {
                    foreach (var name in _nightmareExclusive)
                    {
                        source.Focus.Get<Skill>().AddSkill(nameof(PhantomBook), name);
                    }
                    source.Focus.Get<Skill>().RemoveSkill(nameof(PhantomBook), nameof(Nightmare).ToLower());
                }, false);
            return DSL.Create(sc.Self, pen);
        }
    }
}
