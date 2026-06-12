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
    public partial class BloodSigil : MainProfession
    {
        private readonly ClapStateVar<float> _increase = new(1f);
        private int IncreaseAttack(int origin)
        {
            var res = (int)MathF.Ceiling(origin * _increase.Value);
            _increase.Reset();
            return res;
        }
        private bool BloodBladeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 4;
        }
        [HasAttack(4)]
        [HasRecovery]
        [Labels(Impression.Robust, Strength.Super)]
        private IDSLSourceFile BloodBlade(ISkillContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(4)
                .WriteAttack(IncreaseAttack(6), AttackType.Instance.Physical())
                    .WithBloodSuck(0.75f);
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodLustCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 2;
        }
        [HasBuff]
        [Labels(Impression.Aggressive, Strength.Super)]
        private IDSLSourceFile BloodLust(ISkillContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(2)
                .WriteFree(source =>
                {
                    _increase.Set(1.5f);
                }, true);
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodRecoveryCheck(ISkillContext sc) => true;
        [HasRecovery]
        [Labels(Impression.Conservative, Strength.Useless)]
        private IDSLSourceFile BloodRecovery(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteRecovery(1);
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodShieldCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1;
        }
        [HasDefense]
        [Labels(Impression.Conservative, Strength.Useless)]
        private IDSLSourceFile BloodShield(ISkillContext sc)
        {
            int power = (int)MathF.Ceiling(0.4f * sc.Self.Focus.Get<Health>().HP);
            Pen pen = sf => sf
                .LoseHP(1)
                .WriteDefense(power, new CommonReduction());
            return DSL.Create(sc.Self, pen);
        }
        private bool BloodRageCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Get<Health>().HP > 1 && sc.Self.Focus.Get<Health>().HP <= 5;
        }
        [HasAttack(5)]
        [HasRecovery]
        [Labels(Impression.Robust, Strength.Super)]
        private IDSLSourceFile BloodRage(ISkillContext sc)
        {
            Pen pen = sf => sf
                .LoseHP(1)
                .WriteAttack(IncreaseAttack(5), AttackType.Instance.Physical())
                    .WithBloodSuck(1.5f);
            return DSL.Create(sc.Self, pen);
        }
    }
}
