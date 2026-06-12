using BlacksmithCore.Infra.Attributes.SkillMetadata;
using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Profession;
using BlacksmithCore.Specific.Defense;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Specific.BuiltInProfessions
{
    using DSL = DSLforSkillLogic;
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    public partial class Cannon : MainProfession
    {
        private readonly ClapStateVar<int> _pending = new(0);
        private int AttackPower(int basePower)
        {
            if (_pending.Value <= 0)
            {
                return basePower;
            }

            var result = basePower + _pending.Value;
            _pending.Reset();
            return result;
        }
        private bool StrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(4)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile Strike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(AttackPower(4), AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool DoubleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 2);
        }
        [HasAttack(8)]
        [Labels(Impression.Robust, Strength.Ordinary)]
        private IDSLSourceFile DoubleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(2, ResourceType.Instance.Iron())
                .WriteAttack(AttackPower(8), AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }

        private bool TripleStrikeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3);
        }
        [HasAttack(11)]
        [HasBuff]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile TripleStrike(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(3, ResourceType.Instance.Iron())
                .WriteAttack(AttackPower(11), AttackType.Instance.Physical())
                .WriteResource(0.5f, ResourceType.Instance.Iron())
                .WriteFree(_ => _pending.Increment(), canMove: true);
            return DSL.Create(sc.Self, pen);
        }

        private bool APShellCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1);
        }
        [HasAttack(2)]
        [Labels(Impression.Robust, Strength.Strong)]
        private IDSLSourceFile APShell(ISkillContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron())
                .WriteAttack(AttackPower(2), AttackType.Instance.Physical(), 3)
                    .WithInterupt();
            return DSL.Create(sc.Self, pen);
        }

        private bool CannonBarrelCheck(ISkillContext sc) => true;
        [HasAttack(1)]
        [HasDefense]
        [Labels(Impression.Robust, Strength.Useless)]
        private IDSLSourceFile CannonBarrel(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(2, new CommonReduction())
                .WriteAttack(AttackPower(1), AttackType.Instance.Physical());
            return DSL.Create(sc.Self, pen);
        }
    }
}
