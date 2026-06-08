using BlacksmithCore.Infra.Judgement;
using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapDSL;

namespace BlacksmithCore.Infra.DSL
{
    public interface IDSLSourceFile : IClapDSLSourceFile<Community, Judger, JudgeRuleManager, Intent, IDSLSourceFile>
    {
        public bool IsPassive { get; set; }
        public void Move(Community newOwner);
    }
}
