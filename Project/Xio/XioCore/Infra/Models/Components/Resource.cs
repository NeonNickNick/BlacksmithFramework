
using XioCore.Infra.Models.Core;

namespace XioCore.Infra.Models.Components
{
    public class Resource
    {
        private Dictionary<ResourceType.CEValue, int> _resources = new();
        public Resource()
        {
            Init();
        }
        public void Init()
        {
            _resources = new()
            {
                { ResourceType.Instance.Xio(), 0},
                { ResourceType.Instance.IceShield(), 0},
                { ResourceType.Instance.Light(), 0}
            };
        }
        public bool Check(int need, ResourceType.CEValue resourceType)
        {
            return _resources.ContainsKey(resourceType) && _resources[resourceType] >= need;
        }
        public void Use(int need, ResourceType.CEValue resourceType)
        {
            if (!Check(need, resourceType))
            {
                throw new ArgumentException("UnReachable!");
            }
            _resources[resourceType] -= need;
        }
        public void Gain(int gain, ResourceType.CEValue resourceType)
        {
            _resources[resourceType] += gain;
        }
        public List<(string name, int quantity)> GetView()
        {
            List<(string name, int quantity)> view = new();
            foreach (var key in _resources.Keys)
            {
                view.Add((key.ToString(), _resources[key]));
            }
            return view;
        }
    }
}
