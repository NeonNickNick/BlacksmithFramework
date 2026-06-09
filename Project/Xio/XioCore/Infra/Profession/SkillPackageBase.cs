using ClapInfra.ClapProfession;
using XioCore.Infra.DSL;

namespace XioCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public abstract class SkillPackageBase
        : ClapSkillPackage<ISkillContext, IDSLSourceFile>, ISkillPackage
    {
        public override IDSLSourceFile PassiveSkill(ISkillContext sc)
        {
            return new DSL.SourceFile();
        }
    }
}