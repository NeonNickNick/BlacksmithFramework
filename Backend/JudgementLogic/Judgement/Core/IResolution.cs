namespace Blacksmith.Backend.JudgementLogic.Judgement.Core
{
    public interface IResolution
    {
        public int RemainingRounds { get; set; }
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; }
    }
}