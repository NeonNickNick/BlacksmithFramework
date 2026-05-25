using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace ModExamples.ProphetMod.Defense
{
    public class CrystalWall : DefenseBase
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.RealReduction();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = false;
        public override bool IsDead { get; set; } = false;
        private bool _isHit = false;
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
            if (!_isHit)
            {
                _isHit = true;
                owner.Get<Resource>().Gain(ResourceType.Instance.Crystal(), 1f);
            }
            return (Math.Max(0, attack - Power), Math.Max(attack, Power));
        }
    }
}
