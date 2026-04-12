/*using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Effects;
using Blacksmith.Backend.JudgementLogic.TurnContexts;
 
using System.Collections.Generic;
using UnityEngine;
namespace Blacksmith.Backend.JudgementLogic.Defenses
{
    public class SacrificeReduction : DefenseBase
    {
        public override DefenseName Name { get; set; } = DefenseName.SacrificeReduction;
        public override List<DefenseTag> Tags { get; set; } =
            new()
            {
                DefenseTag.Reduction,
                DefenseTag.Temporary
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
            Power = 0f;
            IsDead = true;
        }

        public override float Work(Body source, Body owner, float attack, AttackType type)
        {
            if (type == AttackType.Physical)
            {
                float resistent = MathF.Min(Power, attack);
                source.LoseHP(resistent / (float)2.0f);
            }

            float temp = Power;
            attack = MathF.Max(0f, attack - temp);
            return attack;
        }
    }
}

*/