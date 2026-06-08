using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Driver
{
    public class GameHistory
    {
        public List<(ISkillContext, ISkillContext)> SkillHistory { get; set; } = new();
        public void Swap()
        {
            int n = SkillHistory.Count;
            for (int i = 0; i < n; ++i)
            {
                var item = SkillHistory[i];
                (item.Item1, item.Item2) = (item.Item2, item.Item1);
            }
        }
    }
}
