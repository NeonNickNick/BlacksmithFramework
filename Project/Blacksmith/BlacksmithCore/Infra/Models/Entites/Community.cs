namespace BlacksmithCore.Infra.Models.Entites
{
    public class Community
    {
        public Body Focus => BodyList[0];
        public List<Body> BodyList { get; private set; }
        private Body? _newFocus;
        public Community()
        {
            BodyList = new() { new(this) };
        }
        public void ReplaceDelayed(Body newFocus)
        {
            _newFocus = newFocus;
        }
        public void Update()
        {
            if (_newFocus != null)
            {
                BodyList[0] = _newFocus;
                _newFocus = null;
            }
        }
    }
}