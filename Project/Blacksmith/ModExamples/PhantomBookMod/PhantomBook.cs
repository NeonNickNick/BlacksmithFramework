using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Attributes.SkillMarkOnly;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;
using ClapInfra.ClapModels.Components;
using ModExamples.PhantomBookMod.Defense;

namespace ModExamples.PhantomBookMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    [IsExperimental]
    public partial class PhantomBook : MainProfession
    {
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
            var fakeSelf = sc.SudoOperations.IsPlayer(sc.Self) ? swapInstance.Enemy : swapInstance.Player;
            var fakeSkill = fakeSelf.Focus.Get<Skill>();
            var fsc = new DefaultSkillContext(swapInstance, expectedSkill, fakeSelf, sc.Param, sc.StringParam);
            if (sc.SudoOperations.GameMetadata.EquipmentSkillNames.Contains(expectedSkill) ||
                sc.SudoOperations.GameMetadata.MainProfessionSkillNames.Contains(expectedSkill) ||
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
            var fakeSelf = sc.SudoOperations.IsPlayer(sc.Self) ? swapInstance.Enemy : swapInstance.Player;
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
               .UseResource(2f, ResourceType.Instance.Dream())
               .WriteEffect(EffectType.Instance.AfterResolutionWritten(), EffectTargetType.Instance.Enemy(), 0, 1,
               (Community source, Body main, EffectEntity effectEntity) =>
               {
                   var tc = main.Get<TurnContext>();
                   tc.Get<AttackResolution>().ForEach(a => a.Clock.DelayRounds++);
                   tc.AddPreprocess<AttackResolution>(a => a.Clock.DelayRounds++, isInfinite: true);
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
            var sandBoxInstance = sc.SudoOperations.DeepCopy(preRounds: 3);
            Body copiedBody = sc.SudoOperations.IsPlayer(sc.Self) ? sandBoxInstance.Player.Focus : sandBoxInstance.Enemy.Focus;
            var resource = copiedBody.Get<Resource>();
            float m = MathF.Min(2f, resource.QueryAll(ResourceType.Instance.Dream()));
            resource.Use(ResourceType.Instance.Dream(), m);
            sc.Self.AddTransform(() =>
            {
                sc.Self.Focus = copiedBody;
            });
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
        [IsExperimental]
        [IsEquipmentSkill]
        private IDSLSourceFile Nightmare(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .LoseHP(5)
                .WriteDefense(0f, new PhysicalImmunity())
                .WriteDefense(6f, new NightmareArmor(owner =>
                {
                    owner.Focus.Get<Skill>().AddSkill(nameof(PhantomBook), nameof(Nightmare));
                    owner.Focus.Get<Skill>().RemovePackage(nameof(Nightmare));
                }))
                .WriteFree(source =>
                {
                    source.Focus.Get<Skill>().RemoveSkill(nameof(PhantomBook), nameof(Nightmare));
                    source.Focus.Get<Skill>().AddPackage(new(new Nightmare()));
                }, false);
            return DSL.Create(sc.Self, pen);
        }
    }
}
