namespace ClapInfra.ClapUnit
{
    public class ClapStateVar<T>
        where T : struct
    {
        private readonly T _initialValue;
        public T Value { get; private set; }
        public ClapStateVar(T value)
        {
            _initialValue = value;
            Value = value;
        }
        public void Reset()
        {
            Value = _initialValue;
        }
        public void Set(T value)
        {
            Value = value;
        }
    }
    public static class ClapStateVarIntExtension
    {
        public static void Increment(this ClapStateVar<int> clapStateVar)
        {
            clapStateVar.Set(clapStateVar.Value + 1);
        }
        public static void Decrement(this ClapStateVar<int> clapStateVar)
        {
            clapStateVar.Set(clapStateVar.Value - 1);
        }
    }
}
