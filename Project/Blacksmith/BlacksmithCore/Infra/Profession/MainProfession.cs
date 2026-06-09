namespace BlacksmithCore.Infra.Profession
{
    public abstract class MainProfession : SkillPackageBase, ISkillPackage
    {
        public MainProfession()
        {
            ProfessionRegistry.AddModOnInit(this);
        }
    }
}
