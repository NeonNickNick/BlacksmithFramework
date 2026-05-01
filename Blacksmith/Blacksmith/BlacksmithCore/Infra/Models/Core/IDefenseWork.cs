using BlacksmithCore.Infra.Models;

namespace BlacksmithCore.Infra.Models.Core
{
    public interface IDefenseWork
    {
        public abstract DefenseType.BEValue Type { get; set; }
        public (int, int) Work(Body source, Body owner, int Attack, AttackType.BEValue type);
    }
}
