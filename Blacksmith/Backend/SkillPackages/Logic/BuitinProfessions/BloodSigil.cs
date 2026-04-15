using Blacksmith.Backend.Backend.SkillPackages.Logic;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.SkillPackages.Core;

namespace Blacksmith.Backend.SkillPackages.Logic.BuitinProfessions
{
    using Pen = Func<DSLforSkillLogic.SourceFile, DSLforSkillLogic.SourceFile>;
    using DSL = DSLforSkillLogic;
    public class BloodSigil : SkillPackageBase
    {
        public override string Name => "bloodsigil";
        public BloodSigil()
        {
            InitializeSkills();
        }
        private bool BloodBladeCheck(ISkillContext sc)
        {
            return sc.Self.Focus.Health.HP > 4;
        }
        private DSL.SourceFile BloodBlade(ISkillContext sc)
        {
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    source.Focus.Health.LoseHP(4);
                })
                .WriteAttack(6, AttackType.Magic)
                    .BloodSuck(0.75f);
            return DSL.Create(sc.Self, pen);
        }
        /*
        private bool BloodLustCheck(ISkillContext sc)
        {

        }
        private DSL.SourceFile BloodLust(ISkillContext sc)
        {

        }
        private bool BloodRecoveryCheck(ISkillContext sc)
        {

        }
        private DSL.SourceFile BloodRecovery(ISkillContext sc)
        {

        }
        private bool BloodShieldCheck(ISkillContext sc)
        {

        }
        private DSL.SourceFile BloodShield(ISkillContext sc)
        {

        }
        private bool BloodRageCheck(ISkillContext sc)
        {

        }
        private DSL.SourceFile BloodRage(ISkillContext sc)
        {

        }*/
    }
}
