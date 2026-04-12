using Blacksmith.Backend.JudgementLogic.Actor;
namespace Blacksmith.Backend.JudgementLogic.Core
{
    public abstract class DefenseBase : IDefenseWork
    {
        public abstract DefenseName Name { get; set; }
        public abstract List<DefenseTag> Tags { get; set; }
        public abstract float Power { get; set; }
        public abstract bool CanMerge { get; set; }
        public abstract bool IsDead { get; set; }
        public abstract void Merge(DefenseBase addition);
        public abstract float Work(Body source, Body owner, float attack, AttackType type);
        public abstract void Update();
    }
}