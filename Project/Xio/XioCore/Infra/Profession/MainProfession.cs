using ClapInfra.ClapProfession;

namespace XioCore.Infra.Profession
{
    public abstract class MainProfession : SkillPackageBase
    {
        public override PackageType PackageType { get; protected set; } = PackageType.Main;

        public MainProfession() : base(PackageType.Main)
        {

        }

    }
}
