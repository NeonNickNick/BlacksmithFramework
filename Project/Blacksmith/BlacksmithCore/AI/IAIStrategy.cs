using BlacksmithCore.Driver;

namespace BlacksmithCore.AI
{
    public interface IAIStrategy
    {
        string Name { get; }
        public void Init(GameInstance gameInstance);
        public (string skillName, int param, string stringParam) ChooseSkill();
    }
}
