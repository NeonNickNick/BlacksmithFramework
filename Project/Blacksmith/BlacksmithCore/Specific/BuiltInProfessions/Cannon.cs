using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defenses;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class Cannon : MainProfession
    {
        private bool StrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        private IDSLSourceFile Strike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(4, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool DoubleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2);
        }
        private IDSLSourceFile DoubleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .WriteAttack(8, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool TripleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        private IDSLSourceFile TripleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteAttack(11, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool APShellCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        private IDSLSourceFile APShell(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(2, AttackType.Instance.Physical(), 3)
                    .WithInterupt();
            return DSL.Create(sc.Self, pen);
        }

        private bool CannonBarrelCheck(ISkillContext sc) => true;
        private IDSLSourceFile CannonBarrel(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(2, new CommonReduction())
                .WriteAttack(1, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
    }
}