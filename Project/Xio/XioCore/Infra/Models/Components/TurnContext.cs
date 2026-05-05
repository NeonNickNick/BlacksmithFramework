using ClapInfra.ClapModels.Components;
using ClapInfra.ClapModels.Entities;
using XioCore.Infra.Models.Components.Resolutions;
using XioCore.Infra.Models.Entities;
namespace XioCore.Infra.Models.Components
{
    public interface IResolution : IClapResolution<Body>
    {
        public float Power { get; set; }
    }
    public class TurnContext : ClapTurnContext<IResolution, Body>, IUpdatePerRound
    {
        public TurnContext() : base(new()
        {
            typeof(UniversalResolution)
        })
        {
        }

        public void Update()
        {
            Get<UniversalResolution>().Clear();
        }

        protected override void ExecuteImpl<TResolution>(Body community, List<TResolution> list, Func<TResolution, bool>? ifProcess)
        {
            foreach (var temp in list)
            {
                if (ifProcess == null || ifProcess(temp))
                {
                    temp.Execute(community);
                }
            }
        }
    }
}
