using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace ModExamples.PhantomBookMod.Defense
{
    public class NightmareArmor : DefenseBase
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.RealArmor();
        public override int Power { get; set; } = 0;
        public int Baseline { get; set; } = 1;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;
        private readonly Action<Community> _callback;
        public NightmareArmor(Action<Community> callback)
        {
            _callback = callback;
        }
        public override void Merge(DefenseBase addition)
        {
            //不会执行
        }
        public override void Update()
        {
            if (Power <= 0)
            {
                IsDead = true;
                _callback(Owner!);
            }
        }

        public override (int, int) Work(Body source, Body owner, int attack, AttackType.CEValue type)
        {
            var res = (Math.Max(0, attack - Power), (int)MathF.Min(attack, Power));
            Power = Math.Max(0, Power - attack);
            return res;
        }
    }
}
