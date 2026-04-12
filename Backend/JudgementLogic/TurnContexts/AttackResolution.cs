using Blacksmith.Backend.JudgementLogic.Actor;
using Blacksmith.Backend.JudgementLogic.Core;
using Blacksmith.Backend.JudgementLogic.Judgement;
using Blacksmith.Backend.JudgementLogic.Judgement.Core;

namespace Blacksmith.Backend.JudgementLogic.TurnContexts
{
    public enum AttackStage
    {
        OnHitDefense,
        OnHitBody
    }
    public class AttackResolution : IResolution
    {
        public int RemainingRounds { get; set; } = 0;
        public AttackType Type { get; set; }
        public AttackTargetType TargetType { get; set; } = AttackTargetType.Enemy;
        public float Power { get; set; }
        public Action<ActorSet> Execute { get; set; }

        private readonly Dictionary<AttackStage, List<Action<ActorSet, Body, AttackResolution>>> _stages = new();
        public AttackResolution() { }
        public AttackResolution(AttackResolution original)
        {
            Type = original.Type;
            Power = original.Power;
            TargetType = original.TargetType;
            Execute = original.Execute;
            _stages = original._stages;
        }
        public void AddStage(AttackStage stage, Action<ActorSet, Body, AttackResolution> action)
        {
            if (!_stages.TryGetValue(stage, out var list))
            {
                list = new();
                _stages[stage] = list;
            }
            list.Add(action);
        }
        public void RunStage(AttackStage stage, ActorSet source, Body target)
        {
            if (_stages.TryGetValue(stage, out var list))
            {
                foreach (var a in list)
                    a(source, target, this);
            }
        }
    }
}