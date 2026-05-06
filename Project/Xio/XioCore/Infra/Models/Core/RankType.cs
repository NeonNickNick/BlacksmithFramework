using XioCore.Infra.Attributes;
using XioCore.Infra.Enum;

namespace XioCore.Infra.Models.Core
{
    public class RankType : XioEnum<RankType>
    {
        [IsXioEnumMember(4)]
        public CEValue Default() => GetCEValue();
        [IsXioEnumMember(3)]
        public CEValue Golden() => GetCEValue();
        [IsXioEnumMember(2)]
        public CEValue Diamond() => GetCEValue();
        [IsXioEnumMember(1)]
        public CEValue BlackHole() => GetCEValue();
        [IsXioEnumMember(0)]
        public CEValue WanXiang() => GetCEValue();
        public Dictionary<int, CEValue> RankDict
        {
            get
            {
                return new()
                {
                    { 0, Default() },
                    { 1, Golden() },
                    { 2, Diamond() },
                    { 3, BlackHole() },
                    { 4, WanXiang() },
                };
            }
        }
    }
}
