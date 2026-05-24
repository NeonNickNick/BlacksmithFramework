using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Components.Resolutions
{
    public enum AttackStage
    {
        OnHitArmorFirstTime,
        OnHitBody,
        OnEnd
    }
    public class AttackResolution : IResolution
    {
        public required Community Source { get; set; }
        public required ClapRoundClock Clock { get; set; }
        public AttackType.CEValue Type { get; set; }
        public float Power { get; set; }
        public Action<Community> Execute { get; set; } = (a) => { };
        public int TotalDamage { get; set; } = 0;

        private readonly Dictionary<AttackStage, List<Action<Community, Body, AttackResolution>>> _stages = new();

        public void AddStage(AttackStage stage, Action<Community, Body, AttackResolution> action)
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