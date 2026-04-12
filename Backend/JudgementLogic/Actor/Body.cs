using Blacksmith.Backend.JudgementLogic.Core;

namespace Blacksmith.Backend.JudgementLogic.Actor
{
    public class Body
    {
        public Skill Skill { get; private set; } = new();
        public Health Health { get; private set; } = new(10, 10);
        public Defense Defense { get; private set; } = new();
        public Resource Resource { get; private set; } = new();
        public Effect Effect { get; private set; } = new();
        public TurnContext TurnContext { get; private set; } = new();
        public void Update()
        {
            Health.Update();
            Defense.Update();
            Effect.Update();
        }
        public void EffectEntityWork(EffectType type)
        {
            Effect.Execute(type, this);
        }
    }
}