using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Specific.Defense
{
    using DSL = DSLforSkillLogic;
    public class ThornReduction : DefenseBase
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.ThornReduction();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;

        public override void Merge(DefenseBase addition)
        {
            //不会被调用
        }
        public override void Update()
        {
            IsDead = true;
        }
        public override (int, int) Work(Body source, Body owner, int attack, AttackType.CEValue type)
        {
            int res = Math.Max(0, attack - Power);
            int absorbed = (int)MathF.Min(attack, Power);
            if (type == AttackType.Instance.Physical())
            {
                DSL.Create(owner.Get<Community>(), sf => sf.WriteAttack(MathF.Ceiling(absorbed / 2f), AttackType.Instance.Magical(), delayRounds: 1)).Compile().Execute(owner.Get<Community>());
            }
            return (res, absorbed);
        }
    }
}