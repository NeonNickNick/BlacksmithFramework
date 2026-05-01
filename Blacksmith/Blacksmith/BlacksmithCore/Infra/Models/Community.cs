namespace BlacksmithCore.Infra.Models
{
    public class Community
    {
        public Body Focus { get; private set; }
        public List<Body> ActorList { get; private set; }
        public Community()
        {
            Focus = new(this);
            ActorList = new() { Focus };
        }
    }
}