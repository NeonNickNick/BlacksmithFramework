using BlacksmithCore.Infra.Attributes.Profession;
using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.BuiltInProfessions;

namespace ModExamples.WineGlassMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    [IsProfessionModifier(nameof(Common))]
    public partial class CommonModifier : ProfessionModifier
    {
        private bool WineGlassCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1.5f);
        }
        [IsProfessionSkill]
        private IDSLSourceFile WineGlass(ISkillContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new WineGlass()));
            Pen pen = sf => sf
                .UseResource(1.5f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    Common.ExcludeAllProfessions(source);
                }, false);
            return DSL.Create(sc.Self, pen);
        }
    }
}
