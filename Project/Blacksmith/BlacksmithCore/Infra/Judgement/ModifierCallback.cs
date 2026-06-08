using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Judgement
{
    public class ModifierCallback : ICallbackOnJudge
    {
        public ClapRoundClock Clock { get; }
        public JudgeStage.CEValue Stage { get; }
        public ModifierOrder ModifierOrder { get; }
        public Action<Community, Community> JudgeRule { get; set; }
        public ModifierCallback(
            Action<Community, Community> judgeRule,
            JudgeStage.CEValue stage,
            ModifierOrder modifierOrder,
            ClapRoundClock clock)
        {
            Clock = clock;
            Stage = stage;
            ModifierOrder = modifierOrder;
            JudgeRule = judgeRule;
        }
    }
}
