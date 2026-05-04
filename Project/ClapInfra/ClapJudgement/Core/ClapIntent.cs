namespace ClapInfra.ClapJudgement.Core
{
    public abstract class ClapIntent<TCommunity>
    {
        public required Action<TCommunity> Execute { get; set; }
    }
}
