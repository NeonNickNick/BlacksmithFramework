using ClapInfra.ClapDSL;
using XioCore.Infra.Models.Entities;
using XioCore.Infra.Models.Judgement;
using XioCore.Infra.Models.Judgement.Core;

namespace XioCore.Infra.DSL
{
    public interface IDSLSourceFile : IClapDSLSourceFile<Body, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
    }
}
