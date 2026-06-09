using BlacksmithCore.Driver;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Profession
{
    public interface ISkillContext
    {
        public ISudoOperations SudoOperations { get; }
        public string SkillName { get; }
        public Community Self { get; }
        public int Param { get; }
        public string StringParam { get; }
    }
    public interface ISudoOperations
    {
        public GameInstance DeepCopy(int preRounds = 0);
        public bool IsPlayer(Community community);
        public IReadOnlyList<(ISkillContext, ISkillContext)> SkillHistory { get; }
        public IGameMetadata GameMetadata { get; }
    }
    public interface IGameMetadata
    {
        public IReadOnlySet<string> MainProfessionSkillNames { get; }
        public IReadOnlySet<string> EquipmentSkillNames { get; }
    }
}
