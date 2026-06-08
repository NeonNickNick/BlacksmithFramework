using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Judgement
{
    public class OverrideCallback : ICallbackOnJudge
    {
        public ClapRoundClock Clock { get; }
        public JudgeStage.CEValue Stage { get; }
        public Action<Community, Community> JudgeRule { get; set; }
        public OverrideCallback(
            Action<Community, Community> judgeRule,
            JudgeStage.CEValue stage,
            ModifierOrder modifierOrder,
            ClapRoundClock clock)
        {
            Clock = clock;
            Stage = stage;
            JudgeRule = judgeRule;
        }
    }
}
