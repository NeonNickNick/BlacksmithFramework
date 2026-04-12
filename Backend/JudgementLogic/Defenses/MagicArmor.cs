using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.TurnContexts;
/*namespace Blacksmith.Backend.JudgementLogic.Defenses
{
    public class MagicArmor : DefenseBase
    {
        public override DefenseName Name { get; set; } = DefenseName.MagicArmor;
        public override List<DefenseTag> Tags { get; set; } =
            new()
            {
                DefenseTag.Armor,
                DefenseTag.Permanent
            };
        public override float Power { get; set; } = (float)0;
        public override bool CanMerge { get; set; } = true;
        public override bool IsDead { get; set; } = false;
        public override void PowerAdd(float addition)
        {
            Power += addition;
        }

        public override void PowerMultiply(float factor)
        {
            Power *= factor;
        }
        public override void Merge(DefenseBase addition)
        {
            Power += addition.Power;
        }
        public override void Update()
        {
            if (Power <= 0f)
            {
                IsDead = true;
            }
        }

        public override float Work(Body source, Body owner, float attack, AttackType type)
        {
            float resistance = MathF.Min(attack, Power);
            EffectEntity effectEntity = new EffectEntity(EffectType.AfterResolutionWritten, new()
            {
                EffectTag.Buff
            }, 1, (float)1.2f);
            effectEntity.Execute = (Body body) =>
            {
                {
                    List<AttackResolution> attackResolutions = body.TurnContext.Exposed.AttackResolutions;
                    foreach (var temp in attackResolutions)
                    {
                        temp.Power *= effectEntity.Power;
                    }
                }
                {
                    List<AttackResolution> attackResolutions = body.TurnContext.Secret.AttackResolutions;
                    foreach (var temp in attackResolutions)
                    {
                        temp.Power *= effectEntity.Power;
                    }
                }
            };
            owner.AddEffect(effectEntity);

            float temp = Power;
            Power = MathF.Max(0f, temp - attack);
            attack = MathF.Max(0f, attack - temp);
            
            return attack;
        }
    }
}
*/