using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;

namespace BlacksmithCore.Infra.Models.Components.Resolutions
{
    public enum AttackStage
    {
        OnHitArmorFirstTime,
        OnEnd
    }
    public class AttackResolution : IResolution
    {
        public Community? Source { get; set; }
        public int DelayRounds { get; set; } = 0;
        public AttackType.BEValue Type { get; set; }
        public float Power { get; set; }
        public Action<Community> Execute { get; set; } = (a) => { };
        public int TotalDamage { get; set; } = 0;

        private readonly Dictionary<AttackStage, List<Action<Community?, Body, AttackResolution>>> _stages = new();
        public AttackResolution() { }

        public void AddStage(AttackStage stage, Action<Community?, Body, AttackResolution> action)
        {
            if (!_stages.TryGetValue(stage, out var list))
            {
                list = new();
                _stages[stage] = list;
            }
            list.Add(action);
        }
        public void RunStage(AttackStage stage, Body target)
        {
            if (_stages.TryGetValue(stage, out var list))
            {
                foreach (var a in list)
                    a(Source, target, this);
            }
        }
    }
}