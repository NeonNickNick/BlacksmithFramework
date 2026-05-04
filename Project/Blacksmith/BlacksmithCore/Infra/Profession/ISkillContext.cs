using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.Profession
{
    public interface ISkillContext
    {
        public string SkillName { get; }
        public Community Self { get; }
        public int Param { get; }
    }
}
