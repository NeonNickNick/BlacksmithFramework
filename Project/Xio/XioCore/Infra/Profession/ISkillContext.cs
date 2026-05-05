

using XioCore.Infra.Models.Entities;

namespace XioCore.Infra.Profession
{
    public interface ISkillContext
    {
        public int Rank { get; }
        public string SkillName { get; }
        public Body Self { get; }
    }
}
