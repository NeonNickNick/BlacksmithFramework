using BlacksmithCore.Backend.JudgementLogic.Judgement;

namespace BlacksmithCore.Backend.Backend.SkillPackages.Logic
{
    public interface ISkillContext
    {
        public string SkillName { get; }
        public ActorSet Self { get; }
        public int Param { get; }
    }
}
