using ClapInfra.ClapEnum;

namespace ClapInfra.ClapUtils
{
    public class EnumRegistry<TIEnum, TMemberAttribute>
        where TIEnum : IClapEnum
        where TMemberAttribute : Attribute, IIsClapEnumMember
    {
        private Dictionary<Type, TIEnum> _supportedEnumDict = new();
        public IReadOnlyDictionary<Type, TIEnum> SupportedEnumDict
            => _supportedEnumDict;
        private Dictionary<Type, Type>? _CEValueTypeDict = null;
        private readonly object _ceValueTypeDictLock = new();
        public IReadOnlyDictionary<Type, Type> CEValueTypeDict
        {
            get
            {
                if (_CEValueTypeDict == null)
                {
                    lock (_ceValueTypeDictLock)
                    {
                        if (_CEValueTypeDict == null)
                        {
                            _CEValueTypeDict = SupportedEnumDict.ToDictionary(s => s.Key, s => s.Value.GetCEValueType());
                        }
                    }
                }
                return _CEValueTypeDict;
            }
        }
        private List<string> _names = new();
        public void RegistEnum(Type type, TIEnum instance)
        {
            if (!SupportedEnumDict.TryGetValue(type, out var value) && !_names.Contains(type.Name))
            {
                _supportedEnumDict[type] = instance;
                _names.Add(type.Name);
            }
            else
            {
                throw new ArgumentException($"TIEnum {type} already exists! Expansion addition failed!");
            }
        }
        public void RegistEnumModifier(TIEnum targetEnum, string name, int priority)
        {
            targetEnum.Create(name, priority);
        }
    }
}
