using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Alchemy : MainProfession
    {
        private bool MidasTouchCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1, true);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile MidasTouch(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron(), true)
                .WriteResource(5, ResourceType.Instance.Gold_Iron());
            return DSL.Create(sc.Self, pen);
        }
    }
}