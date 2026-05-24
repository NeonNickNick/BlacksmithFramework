using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace ModExamples.HolyBookMod.Defense
{
    public class GreyHP : DefenseBase
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.GreyHP();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;
        public override void Merge(DefenseBase addition)
        {
            //不会执行
        }
        public override void Update()
        {
            if (Power <= 0)
            {
                IsDead = true;
            }
        }
        public override (int, int) Work(Body source, Body owner, int attack, AttackType.CEValue type)
        {
            var res = (Math.Max(0, attack - Power), Math.Min(attack, Power));
            Power = Math.Max(0, Power - attack);
            return res;
        }
    }
}
