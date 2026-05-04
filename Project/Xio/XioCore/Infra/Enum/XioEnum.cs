using ClapInfra.ClapEnum;
using XioCore.Infra.Attributes;
namespace XioCore.Infra.Enum
{
    public interface IXioEnum : IClapEnum
    {

    }
    public abstract class XioEnum<T> :
        ClapEnum<T, IsXioEnumMember>, IXioEnum
        where T : XioEnum<T>, new()
    {
    }
}
