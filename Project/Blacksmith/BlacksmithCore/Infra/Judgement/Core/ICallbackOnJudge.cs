using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Judgement.Core
{
    public enum ModifierOrder
    {
        Before,
        After
    }
    public interface ICallbackOnJudge
    {
        public ClapRoundClock Clock { get; }
        public JudgeStage.CEValue Stage { get; }
        public Action<Community, Community> JudgeRule { get; set; }
    }
}
