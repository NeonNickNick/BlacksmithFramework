using System.Reflection;

namespace Blacksmith.AI.Core
{

    public abstract class StrategyParamBase
    {
        protected Dictionary<string, FieldInfo> _fieldDict;

        // 默认构造函数
        protected StrategyParamBase()
        {
            _fieldDict = new Dictionary<string, FieldInfo>();
            BuildFieldDictionary();
        }

        // 拷贝构造函数（protected，供子类调用）
        protected StrategyParamBase(StrategyParamBase other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            _fieldDict = new Dictionary<string, FieldInfo>();

            // 拷贝所有字段值（使用反射自动完成，无需手动逐字段赋值）
            CopyFieldsFrom(other);

            // 重建字段字典（指向当前实例的字段）
            BuildFieldDictionary();
        }

        private void BuildFieldDictionary()
        {
            _fieldDict.Clear();
            var type = this.GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                _fieldDict[field.Name] = field;
            }
        }

        private void CopyFieldsFrom(StrategyParamBase source)
        {
            var type = this.GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = field.GetValue(source);
                field.SetValue(this, value);
            }
        }
    }
    
}