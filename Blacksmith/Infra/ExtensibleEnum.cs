using System.Reflection;
using System.Runtime.CompilerServices;
namespace Blacksmith.Infra
{
    public class ExtensibleEnum<T> where T : new()
    {
        private static readonly Lazy<T> _lazy = new Lazy<T>(() => new T());
        public static T Instance => _lazy.Value;
        public bool IsOpen { get; private set; } = true;
        public struct EEValue : IComparable<EEValue>
        {
            private static int _counter = 0;
            private readonly int _uniqueID;
            private readonly int _priority;
            internal EEValue(ExtensibleEnum<T> factory, int priority)
            {
                if (!factory.IsOpen)
                {
                    throw new ArgumentException("EEValue Factory has been closed!");
                }
                _uniqueID = _counter++;
                _priority = priority;
            }
            public int CompareTo(EEValue other)
            {
                return _priority.CompareTo(other._priority);
            }
            public static bool operator ==(EEValue left, EEValue right)
            {
                return left._uniqueID == right._uniqueID;
            }
            public static bool operator !=(EEValue left, EEValue right)
            {
                return left._uniqueID != right._uniqueID;
            }
            public override bool Equals(object? obj)
            {
                return obj is EEValue other && _uniqueID == other._uniqueID;
            }
            public override int GetHashCode()
            {
                return _uniqueID.GetHashCode();
            }
        }
        private EEValue Create(int priority)
        {
            if (!IsOpen)
            {
                throw new ArgumentException("EEValue Factory has been closed!");
            }
            return new EEValue(this, priority);
        }
        public void CloseFactory() => IsOpen = false;
        protected ExtensibleEnum()
        {
            var type = this.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.NonPublic)
                             .Where(f => f.IsInitOnly)
                             .Where(f => f.FieldType == typeof(int))
                             .ToList();
            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(EEValue) || method.GetParameters().Length != 0)
                {
                    continue; 
                }
                string methodName = method.Name;
                string prefix = "_" + methodName.ToLower();
                var fieldNames = fields.Select(f => f.Name).Where(f => f.StartsWith(prefix)).ToList();
                if(fieldNames.Count != 1)
                {
                    continue;
                }
                int priority = 0;
                try
                {
                    priority = int.Parse(fieldNames[0].Remove(0, prefix.Length));
                }
                catch
                {
                    continue;
                }

                _enumDict[methodName] = Create(priority);
            }

            CloseFactory();
        }
        private static Dictionary<string, EEValue> _enumDict = new();
        public static EEValue GetEEValue([CallerMemberName] string name = "") => _enumDict[name];
    }
    public class AttackType : ExtensibleEnum<AttackType>
    {
        private readonly int _physical256 = 256;
        public EEValue Physical() => GetEEValue();

        private readonly int _magical128 = 128;
        public EEValue Magical() => GetEEValue();

        private readonly int _real0 = 0;
        public EEValue Real() => GetEEValue();
    }
    //模拟外部程序集
    public static class MyAttackTypeExtension
    {
        private static readonly int _mytype64 = 64;
        public static AttackType.EEValue MyType => AttackType.GetEEValue();
    }

}
