using BlacksmithCore.Infra.Models;

namespace BlacksmithCore.Backend.SkillPackages
{
    public interface ISkillContext
    {
        public string SkillName { get; }
        public Community Self { get; }
        public int Param { get; }
    }
}
