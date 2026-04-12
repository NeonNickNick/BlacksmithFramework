using Blacksmith.Backend.JudgementLogic.Actor;

namespace Blacksmith.Backend.JudgementLogic.Core
{
    public interface IDefenseWork
    {
        public abstract DefenseName Name { get; set; }
        public abstract List<DefenseTag> Tags { get; set; }
        public float Work(Body source, Body owner, float Attack, AttackType type);
    }
}
