using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Judgement;
using BlacksmithCore.Infra.Models.Judgement.Core;
using ClapInfra.ClapDSL;

namespace BlacksmithCore.Infra.DSL
{
    public interface IDSLSourceFile : IClapDSLSourceFile<Community, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
    }
}
