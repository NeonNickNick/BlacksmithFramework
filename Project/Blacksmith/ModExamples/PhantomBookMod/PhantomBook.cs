using BlacksmithCore.Driver;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.Resolutions;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
using BlacksmithCore.Infra.Profession;
using ClapInfra.ClapModels.Components;
using ModExamples.HolyBookMod;

namespace ModExamples.PhantomBookMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
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
            swapInstance.Swap();
            var fakeSelf = swapInstance.Player;
            var fakeSkill = fakeSelf.Focus.Get<Skill>();
            var fsc = new DefaultSkillContext(swapInstance, expectedSkill, fakeSelf, sc.Param, sc.StringParam);
            if(expectedSkill == $"{nameof(Association).ToLower()}" || fakeSkill.TryDeclare(fsc.SkillName, fsc) != SkillDeclareResult.Success)
            {
                return false;
            }
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        private IDSLSourceFile Association(ISkillContext sc)
        {
            string expectedSkill = sc.StringParam;
            var swapInstance = sc.SudoOperations.DeepCopy();
            swapInstance.Swap();
            var fakeSelf = swapInstance.Player;
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
        private IDSLSourceFile Hallucinate(ISkillContext sc) 
        {
            Pen pen = sf => sf
               .WriteEffect(EffectType.Instance.AfterTransport(), EffectTargetType.Instance.Enemy(), 0, 1,
               (Community source, Body main, EffectEntity effectEntity) =>
               {
                   main.Get<TurnContext>().Get<ResourceResolution>().RemoveAll(r => r.Type == ResourceType.Instance.Space() || r.Type == ResourceType.Instance.Time());
               });
            return DSL.Create(sc.Self, pen);
        }
    }

}
