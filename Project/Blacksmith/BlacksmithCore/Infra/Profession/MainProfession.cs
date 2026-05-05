using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    public abstract class MainProfession : SkillPackageBase, ISkillPackage
    {
        public override PackageType PackageType { get; protected set; } = PackageType.Main;

        public MainProfession() : base(PackageType.Main)
        {

        }

    }
}
