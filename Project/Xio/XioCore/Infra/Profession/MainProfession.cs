using ClapInfra.ClapProfession;

namespace XioCore.Infra.Profession
{
    public abstract class MainProfession : SkillPackageBase
    {
        public MainProfession()
        {
            ProfessionRegistry.AddModOnInit(this);
        }
    }
}
