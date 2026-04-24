using BlacksmithCore.Backend.JudgementLogic.Judgement;
using BlacksmithCore.FrontendBackendInterface;

namespace BlacksmithCore.AI
{
    public interface IAIStrategy
    {
        string Name { get; }
        public void Init(GameInstance gameInstance);
        (string skillName, int param) ChooseSkill(
            ActorSet self,
            ActorSet opponent);
    }
}
