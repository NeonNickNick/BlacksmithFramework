using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Specific.Defense
{
    public class MagicalImmunity : DefenseBase
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.PhysicalImmunity();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;
        public override void Merge(DefenseBase addition)
        {
            //不会执行
        }
        public override void Update()
        {
            IsDead = true;
        }
        public override (int, int) Work(Body source, Body owner, int attack, AttackType.CEValue type)
        {
            if (type == AttackType.Instance.Magical())
            {
                return (0, attack);
            }
            else
            {
                return (attack, 0);
            }
        }
    }
}
