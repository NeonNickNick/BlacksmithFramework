namespace ClapInfra.ClapUnit
{
    public class ClapSharedFlag
    {
        private int _times = 0;
        public bool IsActive => _times <= 0;
        public void Disable()
        {
            _times++;
        }
        public void Enable()
        {
            if (_times <= 0)
            {
                return;
            }
            _times--;
        }
    }
}
