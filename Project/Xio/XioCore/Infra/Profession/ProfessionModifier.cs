using ClapInfra.ClapProfession;

namespace XioCore.Infra.Profession
{
    public abstract class ProfessionModifier : SkillPackageBase
    {
        public override PackageType PackageType { get; protected set; } = PackageType.Modifier;
        public ProfessionModifier() : base(PackageType.Main)
        {

        }
    }
}
