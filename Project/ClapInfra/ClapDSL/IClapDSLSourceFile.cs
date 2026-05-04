using ClapInfra.ClapJudgement;
using ClapInfra.ClapJudgement.Core;

namespace ClapInfra.ClapDSL
{
    public interface IClapDSLSourceFile<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile>
        where TJudger : ClapJudger<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile>
        where TIDSLSourceFile : IClapDSLSourceFile<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile>
        where TIntent : ClapIntent<TCommunity>
        where TJudgeRuleManager : ClapJudgeRuleManager<TCommunity>, new()
    {
        public TIntent Compile(TJudger? judger = null);
    }
}
