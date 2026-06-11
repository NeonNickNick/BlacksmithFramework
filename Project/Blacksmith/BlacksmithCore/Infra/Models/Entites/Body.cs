using BlacksmithCore.Infra.Models.Components;
using ClapInfra.ClapModels.Entities;

namespace BlacksmithCore.Infra.Models.Entites
{
    public class Body : ClapBody
    {
        private string _name;
        public Body(Community community, string name) : base(new()
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
            _name = name;
        }
        public void Reset()
        {
            Get<Skill>().Reset();
            Get<Health>().Reset();
            Get<Defense>().Reset();
            Get<Resource>().Reset();
            Get<Effect>().Reset();
            Get<TurnContext>().Reset();
        }
        public BodyView GetView()
        {
            return new()
            {
                BodyName = _name,
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
        public required string BodyName { get; set; }
        public required List<string> ProfessionNames { get; set; }
        public required int HP { get; set; }
        public required int MHP { get; set; }
        public required List<(string name, int power)> DefenseView { get; set; }
        public required List<(string name, float quantity)> ResourcesView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureAttackView { get; set; }
        public required List<(string name, int delayRounds, int power)> FutureDefenseView { get; set; }
    }
}