namespace BlacksmithCore.Infra.Models.Components
{
    public class Health
    {
        private int _hp;
        private bool _killed = false;
        public bool IsKilled => _killed;
        public int HP
        {
            get => _hp;
            set
            {
                if (value <= 0)
                {
                    _killed = true;
                }
                _hp = value;
            }
        }
        public int MHP { get; set; }
        public void Reset()
        {
            _hp = 10;
            MHP = 10;
            _killed = false;
        }
        public Health(int hp, int mhp)
        {
            HP = hp;
            MHP = mhp;
        }
        public void GainHP(int addition)
        {
            if (_killed)
            {
                return;
            }
            HP = (int)MathF.Min(MHP, HP + addition);
        }
        public void GainMHP(int addition)
        {
            if (_killed)
            {
                return;
            }
            MHP += addition;
        }
        public void LoseHP(int loss)
        {
            HP = HP - loss;
        }
        public void LoseMHP(int loss)
        {
            MHP = Math.Max(0, MHP - loss);
            HP = Math.Min(MHP, HP);
        }
    }
}