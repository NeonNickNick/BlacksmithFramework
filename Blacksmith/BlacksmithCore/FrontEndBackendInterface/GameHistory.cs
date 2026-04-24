using BlacksmithCore.Backend.Backend.SkillPackages.Logic;
using BlacksmithCore.FrontendBackendInterface.Web;

namespace BlacksmithCore.FrontendBackendInterface
{
    public class GameHistory
    {
        public List<(ISkillContext, ISkillContext)> SkillHistory { get; set; } = new();
        public List<WebTurnRecord> Turns { get; set; } = new();
        public void Swap()
        {
            SkillHistory = SkillHistory.Select(s => (s.Item2, s.Item1)).ToList();
            Turns = Turns
                .Select(t => new WebTurnRecord(
                    t.Index,
                    new WebActionRecord(t.Enemy.SkillName, t.Enemy.Param),
                    new WebActionRecord(t.Player.SkillName, t.Player.Param),
                    t.Result))
                .ToList();
        }
    }
}
