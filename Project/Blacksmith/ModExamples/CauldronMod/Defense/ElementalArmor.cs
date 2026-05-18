using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace ModExamples.CauldronMod.Defense
{
    public class ElementalArmor : DefenseBase
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.CommonArmor();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = true;
        public override bool IsDead { get; set; } = false;
        private Action _callback;
        public ElementalArmor(Action callback)
        {
            _callback = callback;
        }
        public override void Merge(DefenseBase addition)
        {
            //什么都不做即可
        }
        public override void Update()
        {
            Power -= 2;
            if(Power <= 0)
            {
                IsDead = true;
                _callback();
            }
        }
        public override (int, int) Work(Body source, Body owner, int attack, AttackType.CEValue type)
        {
            (int, int) res;
            if(type == AttackType.Instance.Magical())
            {
                res = ((int)MathF.Max(0, attack - Power), (int)MathF.Min(attack, Power));
            }
            else
            {
                res = (0, attack);
            } 
            Power = (int)MathF.Max(0, Power - attack);
            return res;
        }
    }
}
