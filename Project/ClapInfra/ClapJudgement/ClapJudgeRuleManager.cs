namespace ClapInfra.ClapJudgement
{
    public abstract class ClapJudgeRuleManager<TCommunity>
    {
        public abstract Action<TCommunity, TCommunity> GetRule();
    }
}
