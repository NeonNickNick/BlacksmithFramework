using ClapInfra.ClapModels.Entities;
using XioCore.Infra.Models.Components;
using XioCore.Infra.Models.Judgement;

namespace XioCore.Infra.Models.Entities
{
    public class Body : ClapBody
    {
        public JudgeRuleManager manager = new();

        public Body() : base(new()
        {
            new Level(),
            new Skill(),
            new Defense(),
            new Resource(),
            new TurnContext()
        })
        {
        }
        public BodyView GetView()
        {
            return new()
            {
                Round = manager.Round,
                InnerRound = manager.InnerRound,
                Rank = Get<Level>().Rank,
                InnerLevel = Get<Level>().InternalLevel,
                ResourceView = Get<Resource>().GetView()
            };
        }
    }
    public class BodyView
    {
        public required int Round { get; set; }
        public required int InnerRound { get; set; }
        public required int Rank { get; set; }
        public required int InnerLevel { get; set; }
        public required List<(string name, int quantity)> ResourceView { get; set; }
    }
}
