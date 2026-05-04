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
        private Dictionary<Type, Type>? _BEValueTypeDict = null;
        public IReadOnlyDictionary<Type, Type> BEValueTypeDict
        {
            get
            {
                if (_BEValueTypeDict == null)
                {
                    _BEValueTypeDict = SupportedEnumDict.ToDictionary(s => s.Key, s => s.Value.GetBEValueType());
                }
                return _BEValueTypeDict;
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
