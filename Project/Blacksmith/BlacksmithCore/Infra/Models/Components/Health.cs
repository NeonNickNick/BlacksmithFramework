namespace BlacksmithCore.Infra.Models.Components
{
    public class Health
    {
        private int _hp;
        private bool _killed = false;
        public int HP
        {
            get => _hp;
            set
            {
                if(value <= 0)
                {
                    _killed = true;
                }
                _hp = value;
            }
        }
        public int MHP { get; private set; }
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
            MHP = (int)MathF.Max(0, MHP - loss);
            HP = (int)MathF.Min(MHP, HP);
        }
    }
}