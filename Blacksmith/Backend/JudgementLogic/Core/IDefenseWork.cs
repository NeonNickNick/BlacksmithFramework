using Blacksmith.Backend.JudgementLogic.Actor;

namespace Blacksmith.Backend.JudgementLogic.Core
{
    public interface IDefenseWork
    {
        public abstract DefenseType Type { get; set; }
        public (int, int) Work(Body source, Body owner, int Attack, AttackType type);
    }
}
