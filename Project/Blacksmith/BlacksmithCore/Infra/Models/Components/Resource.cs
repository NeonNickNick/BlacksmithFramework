using BlacksmithCore.Infra.Models.Core;

namespace BlacksmithCore.Infra.Models.Components
{

    public class Resource
    {
        private class ResourceTemplate
        {
            public ResourceType.CEValue CommonType { get; }
            public ResourceType.CEValue GoldType { get; }
            public float Common { get; set; } = 0;
            public float Gold { get; set; } = 0;
            public void Reset()
            {
                Common = 0;
                Gold = 0;
            }
            public ResourceTemplate(ResourceType.CEValue commonType, ResourceType.CEValue goldType)
            {
                CommonType = commonType;
                GoldType = goldType;
            }
            public bool Check(float need, bool ifCommonOnly = false)
            {
                float temp = Common;
                if (!ifCommonOnly)
                {
                    temp += Gold;
                }
                return temp >= need;
            }
            public void Use(float need, bool ifCommonOnly = false)
            {
                if (!Check(need, ifCommonOnly))
                {
                    throw new ArgumentException("Unreachable5!");
                }
                if (!ifCommonOnly)
                {
                    if (need <= Gold)
                    {
                        Gold -= need;
                    }
                    else
                    {
                        Common -= need - Gold;
                        Gold = 0;
                    }
                }
                else
                {
                    Common -= need;
                }
            }
            public void Gain(ResourceType.CEValue type, float add)
            {
                if (type == CommonType)
                {
                    Common += add;
                }
                else if (type == GoldType)
                {
                    Gold += add;
                }
                else
                {
                    throw new ArgumentException("Unreachable4!");
                }
            }
        }
        private Dictionary<ResourceType.CEValue, ResourceTemplate> _resources = new();
        private static IReadOnlyDictionary<string, ResourceType.CEValue> _dictRef => ResourceType.EnumDict;
        public Resource()
        {
            List<string> enumNames = _dictRef!.Keys.ToList();
            string prefix = "Gold_";
            List<string> golds = enumNames.Where(e => e.StartsWith(prefix)).ToList();
            enumNames.RemoveAll(golds.Contains);
            foreach (var gold in golds)
            {
                string commonName = gold.Remove(0, prefix.Length);
                if (enumNames.Contains(commonName))
                {
                    var shareTemplate = new ResourceTemplate(_dictRef[commonName], _dictRef[gold]);
                    _resources[_dictRef[commonName]] = shareTemplate;
                    _resources[_dictRef[gold]] = shareTemplate;
                    enumNames.Remove(commonName);
                }
                else
                {
                    throw new ArgumentException($"ResourceType {gold} has no paired general resourceType!");
                }
            }
            foreach (var rest in enumNames)
            {
                var template = new ResourceTemplate(_dictRef[rest], _dictRef[rest]);
                _resources[_dictRef[rest]] = template;
            }
        }
        public void Reset()
        {
            foreach (var tmp in _resources.Values)
            {
                tmp.Reset();
            }
        }
        public bool Check(ResourceType.CEValue type, float need, bool ifCommonOnly = false)
        {
            return _resources[type].Check(need, ifCommonOnly);
        }
        public void Use(ResourceType.CEValue type, float need, bool ifCommonOnly = false)
        {
            _resources[type].Use(need, ifCommonOnly);
        }
        public void Gain(ResourceType.CEValue type, float need)
        {
            _resources[type].Gain(type, need);
        }
        public float Query(ResourceType.CEValue type)
        {
            if (type == _resources[type].CommonType)
            {
                return _resources[type].Common;
            }
            else
            {
                return _resources[type].Gold;
            }
        }
        public float QueryAll(ResourceType.CEValue type)
        {
            return _resources[type].Gold + _resources[type].Common;
        }
        public float QuerySpecific()
        {
            float res = 0;
            foreach (var name in _resources.Keys)
            {
                if (name == ResourceType.Instance.Iron() ||
                    name == ResourceType.Instance.Gold_Iron() ||
                    name == ResourceType.Instance.Space() ||
                    name == ResourceType.Instance.Time())
                {
                    continue;
                }
                res += _resources[name].Gold + _resources[name].Common;
            }
            return res;
        }
        public List<(string name, float quantity)> GetView()
        {
            List<(string name, float quantity)> view = new();
            foreach (var key in _resources.Keys)
            {
                view.Add((key.ToString(), Query(key)));
            }
            return view;
        }
    }
}
