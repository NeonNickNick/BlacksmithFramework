namespace ClapInfra.ClapUnit
{
    public class ClapRoundClock
    {
        private int _delayRounds;
        private int _remainingRounds;
        private bool _isInfinite;
        public int DelayRounds { get => _delayRounds; set => _delayRounds = value; }
        public int RemainingRounds { get => _remainingRounds; set => _remainingRounds = value; }
        public bool IsRinging => _delayRounds == 0 && _remainingRounds > 0;
        public bool IsDead => _remainingRounds <= 0;
        public ClapRoundClock(int delayRounds = 0, int remainingRounds = 1, bool isInfinite = false)
        {
            _delayRounds = delayRounds;
            _remainingRounds = remainingRounds;
            _isInfinite = isInfinite;
        }
        public void RoundPass()
        {
            if(_delayRounds > 0)
            {
                _delayRounds--;
                return;
            }
            if (!_isInfinite)
            {
                _remainingRounds--;
            }
        }
    }
}
