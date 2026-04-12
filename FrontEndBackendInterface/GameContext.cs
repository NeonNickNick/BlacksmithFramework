using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.SkillPackages.Logic;

namespace Blacksmith.FrontendBackendInterface
{
    public interface ISkillChoose
    {
        public SkillDeclareResult TryDeclare(string skillName, int param);
        public void Declare(string skillName, int param);
    }
    public interface IActorSetState
    {
        public int HP { get; }
        public int MHP { get; }
        public float Iron { get; }
    }
    public class TestActorSetState : IActorSetState
    {
        public ActorSet As { get; set; }
        public int HP => As.Focus.Health.HP;
        public int MHP => As.Focus.Health.MHP;
        public float Iron => As.Focus.Resource.QueryCommon(ResourceType.Iron);
    }
    public class GameContext
    {
        public ISkillChoose SkillChoose { get; set; }
        public IActorSetState PlayerActorSetState { get; set; }
        public IActorSetState EnemyActorSetState { get; set; }
    }
}