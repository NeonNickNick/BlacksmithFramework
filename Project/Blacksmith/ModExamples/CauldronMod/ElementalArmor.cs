using BlacksmithCore.Infra.Attributes.SkillClassification;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;

namespace ModExamples.CauldronMod
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class ElementalArmor : MainProfession
    {
        private bool HammerCheck(ISkillContext sc) => true;
        [IsAttack]
        private IDSLSourceFile Hammer(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteAttack(6f, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
        private bool GuardCheck(ISkillContext sc) => true;
        private IDSLSourceFile Guard(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(8f, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }
    }
}
