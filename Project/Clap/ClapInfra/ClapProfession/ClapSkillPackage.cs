namespace ClapInfra.ClapProfession
{
    public interface IClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        public HashSet<string> AvailableSkillNames { get; }
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker { get; }
        public Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> SkillSourceFileGenerator { get; }
        public abstract TIDSLSourceFile PassiveSkill(TISkillContext sc);
    }

    public abstract class ClapSkillPackage<TISkillContext, TIDSLSourceFile>
        : IClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        protected readonly HashSet<string> _availableSkillNames = new();
        protected readonly Dictionary<string, Func<TISkillContext, bool>> _skillChecker = new();
        protected readonly Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> _skillSourceFileGenerator = new();

        public HashSet<string> AvailableSkillNames => _availableSkillNames;
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker => _skillChecker;
        public Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;

        protected ClapSkillPackage()
        {
            RegistSkills();
        }

        protected void RegistSkill(
            string skillName,
            Func<TISkillContext, bool> checker,
            Func<TISkillContext, TIDSLSourceFile> generator)
        {
            _availableSkillNames.Add(skillName);
            _skillChecker.Add(skillName, checker);
            _skillSourceFileGenerator.Add(skillName, generator);
        }

        /// <summary>
        /// Overridden by source-generated partial class to register skills without reflection.
        /// </summary>
        protected virtual void RegistSkills() { }

        public abstract TIDSLSourceFile PassiveSkill(TISkillContext sc);
    }
}
