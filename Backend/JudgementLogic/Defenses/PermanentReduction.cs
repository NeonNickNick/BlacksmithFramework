using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
namespace Blacksmith.Backend.JudgementLogic.Defenses
{
    public class PermanentReduction : DefenseBase
    {
        public override DefenseName Name { get; set; } = DefenseName.PermanentReduction;
        public override List<DefenseTag> Tags { get; set; } =
            new()
            {
                DefenseTag.Reduction,
                DefenseTag.Permanent
            };
        public override float Power { get; set; } = (float)0;
        public override bool CanMerge { get; set; } = true;
        public override bool IsDead { get; set; } = false;
      
        public override void Merge(DefenseBase addition)
        {
            Power += addition.Power;
        }
        public override void Update()
        {
            if(Power <= 0f)
            {
                IsDead = true;
            }
        }

        public override float Work(Body source, Body owner, float attack, AttackType type)
        {
            float temp = Power;
            Power = MathF.Max(0f, temp - attack);
            attack = MathF.Max(0f, attack - temp);
            return attack;
        }
    }
}