namespace BlacksmithCore.Backend.SkillPackages.Core
{
    public abstract class ProfessionModifier : SkillPackageBase
    {
        public override PackageType PackageType { get; protected set; } = PackageType.Modifier;
        public ProfessionModifier() : base(PackageType.Modifier)
        {

        }
    }
}
