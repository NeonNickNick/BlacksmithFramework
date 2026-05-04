using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defenses;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public class Driver : MainProfession
    {
        public override IDSLSourceFile PassiveSkill(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(1, new RealReduction())
                .WriteDefense((int)MathF.Min(5, sc.Self.Focus.Get<Resource>().Query(ResourceType.Instance.Time()) * 2), new RealReduction());
            return DSL.Create(sc.Self, pen);
        }
        private bool SpaceAttackCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), sc.Param);
        }
        private IDSLSourceFile SpaceAttack(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Instance.Space())
                .WriteAttack(12 * sc.Param, AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool Space2TimeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Space(), 1);
        }
        private IDSLSourceFile Space2Time(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Space())
                .WriteResource(1, ResourceType.Instance.Time())
                .WriteDefense(3, new RealReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool Time2SpaceCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Time(), 1);
        }
        private IDSLSourceFile Time2Space(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Time())
                .WriteResource(1, ResourceType.Instance.Space())
                .WriteDefense(3, new RealReduction());
            return DSL.Create(sc.Self, pen);
        }

        private bool SpaceBarrierCheck(ISkillContext sc)
        {
            return sc.Param > 0 && sc.Param <= 5 && sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), sc.Param);
        }
        private IDSLSourceFile SpaceBarrier(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(sc.Param, ResourceType.Instance.Iron())
                .WriteDefense(5.5f * sc.Param - 0.5f * sc.Param * sc.Param, new RealReduction());
            return DSL.Create(sc.Self, pen);
        }
    }
}