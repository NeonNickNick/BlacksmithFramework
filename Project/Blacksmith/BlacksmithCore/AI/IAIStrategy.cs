using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.AI
{
    public interface IAIStrategy
    {
        string Name { get; }
        public void Init(GameInstance gameInstance);
        (string skillName, int param) ChooseSkill(
            Community self,
            Community opponent);
    }
}
