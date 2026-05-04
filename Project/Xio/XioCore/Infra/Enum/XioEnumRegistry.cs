using ClapInfra.ClapUtils;
using XioCore.Infra.Attributes;

namespace XioCore.Infra.Enum
{
    public static class XioEnumRegistry
    {
        private static EnumRegistry<IXioEnum, IsXioEnumMember> _enumRegistry = new();
        public static IReadOnlyDictionary<Type, IXioEnum> SupportedEnumDict
            => _enumRegistry.SupportedEnumDict;
        public static IReadOnlyDictionary<Type, Type> BEValueTypeDict
            => _enumRegistry.BEValueTypeDict;
        public static void RegistXioEnum(Type type, IXioEnum instance)
            => _enumRegistry.RegistEnum(type, instance);
        public static void RegistXioEnumModifier(IXioEnum targetEnum, string name, int priority)
            => _enumRegistry.RegistEnumModifier(targetEnum, name, priority);
    }
}
