using System.Reflection;
using System.Runtime.CompilerServices;
namespace ClapInfra.ClapEnum
{
    public interface IClapEnum
    {
        public abstract Type GetCEValueType();
        public abstract void Create(string name, int priority);
    }
    public abstract class ClapEnum : IClapEnum
    {
        protected static bool _isOpen = true;
        public static void CloseFactory() => _isOpen = false;
        public abstract Type GetCEValueType();
        public abstract void Create(string name, int priority);
    }
    public interface IIsClapEnumMember
    {
        public int Priority { get; }
    }
    public abstract class ClapEnum<T, TMemberAttribute> : ClapEnum
        where T : ClapEnum<T, TMemberAttribute>, new()
        where TMemberAttribute : Attribute, IIsClapEnumMember
    {
        //实际上可断言一定是先调用构造函数，此时已经不是null
        public static T Instance { get; private set; } = null!;

        public struct CEValue : IComparable<CEValue>
        {
            private static int _counter = 0;
            private readonly int _uniqueID;
            public readonly int _priority;
            internal CEValue(int priority)
            {
                if (!_isOpen)
                {
                    throw new ArgumentException("CEValue Factory has been closed!");
                }
                _uniqueID = _counter++;
                _priority = priority;
            }
            public int CompareTo(CEValue other)
            {
                return _priority.CompareTo(other._priority);
            }
            public static bool operator ==(CEValue left, CEValue right)
            {
                return left._uniqueID == right._uniqueID;
            }
            public static bool operator !=(CEValue left, CEValue right)
            {
                return left._uniqueID != right._uniqueID;
            }
            public override bool Equals(object? obj)
            {
                return obj is CEValue other && _uniqueID == other._uniqueID;
            }
            public override int GetHashCode()
            {
                return _uniqueID.GetHashCode();
            }
            public override string ToString()   //用来显示可读名字
            {
                foreach (var kvp in _enumDict)
                {
                    if (kvp.Value._uniqueID == _uniqueID) return kvp.Key;
                }
                return base.ToString() ?? "";
            }
        }
        public override Type GetCEValueType()
        {
            return typeof(CEValue);
        }
        public override void Create(string name, int priority)
        {
            if (!_isOpen)
            {
                throw new ArgumentException("CEValue Factory has been closed!");
            }
            //这里选择直接覆盖。程序启动时就已经被构造
            //情况与技能包不同，技能包每次使用都需要创建实例，不便于指定构造参数来应用Modifier
            //因此采用的方法是在构造函数插入一个修改阶段
            //而Enum是全局单例，干脆在初始化阶段就修改
            _enumDict[name] = new CEValue(priority);
        }
        protected ClapEnum()
        {
            if (!_isOpen)
            {
                return;
            }
            Instance = (T)this;
            var type = GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var metaData = method.GetCustomAttribute<TMemberAttribute>();
                if (method.ReturnType != typeof(CEValue) ||
                    method.GetParameters().Length != 0 ||
                    metaData == null)
                {
                    continue;
                }
                string methodName = method.Name;
                Create(methodName, metaData.Priority);
            }
        }
        private static readonly Dictionary<string, CEValue> _enumDict = new();
        public static IReadOnlyDictionary<string, CEValue> EnumDict => _enumDict;
        public static CEValue GetCEValue([CallerMemberName] string name = "") => _enumDict[name];
    }

}
