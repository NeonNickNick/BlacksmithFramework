namespace BlacksmithCore.Infra.Profession
{
    public abstract class ProfessionModifier : SkillPackageBase
    {
        public abstract void Bind(MainProfession package);
    }
}
