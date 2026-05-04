using BlacksmithCore.Infra.DSL;
using ClapInfra.ClapProfession;

namespace BlacksmithCore.Infra.Profession
{
    using DSL = DSLforSkillLogic;
    public interface ISkillPackage : ISkillPackage<ISkillContext, IDSLSourceFile>
    {
    }
}
