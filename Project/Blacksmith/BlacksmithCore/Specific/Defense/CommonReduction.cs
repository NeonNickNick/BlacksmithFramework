using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
namespace BlacksmithCore.Specific.Defense
{
    public class CommonReduction : DefenseBase
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.CommonReduction();
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
            return (Math.Max(0, attack - Power), Math.Min(attack, Power));
        }
    }
}