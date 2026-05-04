using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions;

namespace ModExamples
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    [IsProfessionModifier(nameof(Common))]
    public class CommonModifier : ProfessionModifier
    {
        private bool HolyBookCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2f);
        }
        private DSL.SourceFile HolyBook(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new HolyBook());
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    Common.ExcludeAllProfessions(source);
                });
            return DSL.Create(sc.Self, pen);
        }
    }
}
