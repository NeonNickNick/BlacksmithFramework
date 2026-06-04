using ClapInfra.ClapUnit;

namespace BlacksmithCore.Specific.Combat
{
    /// <summary>
    /// Flat bonus applied to the next attack power written by a profession package.
    /// One <see cref="ApplyToAttackPower"/> call consumes all pending bonus.
    /// </summary>
    public sealed class NextAttackAdditiveBonus
    {
        private readonly ClapStateVar<int> _pending = new(0);

        public int Pending => _pending.Value;
        public bool HasPending => _pending.Value > 0;

        public void Grant(int amount)
        {
            if (amount > 0)
            {
                _pending.Set(_pending.Value + amount);
            }
        }

        public int ApplyToAttackPower(int basePower)
        {
            if (_pending.Value <= 0)
            {
                return basePower;
            }

            var result = basePower + _pending.Value;
            _pending.Reset();
            return result;
        }
    }
}
