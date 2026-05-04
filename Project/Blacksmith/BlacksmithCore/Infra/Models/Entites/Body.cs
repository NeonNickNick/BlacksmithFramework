using BlacksmithCore.Infra.Models.Components;
using ClapInfra.ClapModels.Entities;

namespace BlacksmithCore.Infra.Models.Entites
{
    public class Body : ClapBody
    {
        public Body(Community community) : base(new()
        {
            community,
            new Skill(),
            new Health(10, 10),
            new Defense(),
            new Resource(),
            new Effect(),
            new TurnContext()
        })
        {
        }
        public BodyView GetView()
        {
            return new()
            {
                ProfessionNames = Get<Skill>().GetView(),
                HP = Get<Health>().HP,
                MHP = Get<Health>().MHP,
                DefenseView = Get<Defense>().GetView(),
                ResourcesView = Get<Resource>().GetView(),
                FutureAttackView = Get<TurnContext>().GetFutureAttackView(),
                FutureDefenseView = Get<TurnContext>().GetFutureDefenseView()
            };
        }
    }
    public class BodyView
    {
        public required List<string> ProfessionNames { get; set; }
        public required int HP { get; set; }
        public required int MHP { get; set; }
        public required List<(string name, int power)> DefenseView { get; set; }
        public required List<(string name, float quantity)> ResourcesView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureAttackView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureDefenseView { get; set; }
    }
}