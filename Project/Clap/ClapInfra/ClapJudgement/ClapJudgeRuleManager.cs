namespace ClapInfra.ClapJudgement
{
    public abstract class ClapJudgeRuleManager<TCommunity>
    {
        public abstract void Judge(TCommunity player, TCommunity enemy);
    }
}
