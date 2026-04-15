using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;

namespace Blacksmith.Backend.JudgementLogic.Entities
{
    public class EffectEntity
    {
        public readonly EffectType Type;
        public List<EffectTag> Tags { get; set; }
        public int DelayTimes { get; set; } = 0;
        public int RemainingTimes { get; set; }
        public IResolution Resolution { get; set; }
        public Action<Body> Execute { get; set; }
        public EffectEntity(EffectType type, List<EffectTag> tags, int remainingTimes, IResolution resolution)
        {
            Type = type;
            Tags = tags;
            RemainingTimes = remainingTimes;
            Resolution = resolution;
        }
    }
}