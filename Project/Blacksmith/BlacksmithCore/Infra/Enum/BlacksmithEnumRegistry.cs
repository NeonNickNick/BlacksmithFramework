using BlacksmithCore.Infra.Attributes;
using ClapInfra.ClapUtils;

namespace BlacksmithCore.Infra.Enum
{
    public static class BlacksmithEnumRegistry
    {
        private static EnumRegistry<IBlacksmithEnum, IsBlacksmithEnumMember> _enumRegistry = new();
        public static IReadOnlyDictionary<Type, IBlacksmithEnum> SupportedEnumDict
            => _enumRegistry.SupportedEnumDict;
        public static IReadOnlyDictionary<Type, Type> BEValueTypeDict
            => _enumRegistry.BEValueTypeDict;
        public static void RegistBlacksmithEnum(Type type, IBlacksmithEnum instance)
            => _enumRegistry.RegistEnum(type, instance);
        public static void RegistBlacksmithEnumModifier(IBlacksmithEnum targetEnum, string name, int priority)
            => _enumRegistry.RegistEnumModifier(targetEnum, name, priority);
    }
}
