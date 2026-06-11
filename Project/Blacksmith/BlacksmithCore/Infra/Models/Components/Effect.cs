using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using BlacksmithCore.Infra.Models.Particular;
using ClapInfra.ClapModels.Entities;
namespace BlacksmithCore.Infra.Models.Components
{
    public class Effect : IUpdatePerRound
    {
        private readonly List<EffectEntity> _effects = new();
        public void Reset()
        {
            _effects.Clear();
        }
        public void Add(EffectEntity effectEntity)
        {
            _effects.Add(effectEntity);
        }
        public void AddRange(List<EffectEntity> effectEntities)
        {
            _effects.AddRange(effectEntities);
        }
        public void Execute(EffectType.CEValue type, Body body)
        {
            IEnumerable<EffectEntity> tempList = _effects.Where(e => e.Type == type);
            foreach (var temp in tempList)
            {
                if (temp.Clock.IsRinging)
                {
                    temp.Execute(body);
                }
            }
        }
        public void Update()
        {
            _effects.ForEach(e => e.Clock.RoundPass());
            _effects.RemoveAll(e => e.Clock.IsDead);
        }
        public List<EffectEntity> Get()
        {
            return _effects;
        }
    }
}