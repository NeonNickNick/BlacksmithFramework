using System.Collections;

namespace ClapInfra.ClapModels.Components
{
    public interface IClapResolution<TCommunity>
    {
        public Action<TCommunity> Execute { get; set; }
    }
    public abstract class ClapTurnContext<TIResolution, TCommunity>
        where TIResolution : IClapResolution<TCommunity>
    {
        protected Dictionary<Type, IList> _resolutionLists = new();
        protected ClapTurnContext(HashSet<Type> resolutionTypes)
        {
            if (resolutionTypes == null)
            {
                throw new ArgumentNullException(nameof(resolutionTypes));
            }
            foreach (var type in resolutionTypes)
            {
                if (type == null)
                {
                    throw new ArgumentException("Resolution type cannot be null!");
                }
                if (!(type.IsAssignableTo(typeof(TIResolution))))
                {
                    throw new ArgumentException($"Resolution must derive from {nameof(TIResolution)}");
                }
                Type listType = typeof(List<>).MakeGenericType(type);
                _resolutionLists[type] = (IList)Activator.CreateInstance(listType)!;
            }
        }
        public List<TResolution> Get<TResolution>()
            where TResolution : TIResolution
        {
            if (_resolutionLists.TryGetValue(typeof(TResolution), out var list))
            {
                return (List<TResolution>)list;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Resolution type {typeof(TResolution).Name} is not registered in the context.");
            }
        }
        public void WriteResolution(TIResolution resolution)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            if (_resolutionLists.TryGetValue(resolution.GetType(), out var list))
            {
                list.Add(resolution);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Resolution type {resolution.GetType().Name} is not registered in the context.");
            }
        }
        public void Execute<TResolution>(TCommunity community)
            where TResolution : TIResolution
        {
            ExecuteImpl(community, Get<TResolution>());
        }
        protected abstract void ExecuteImpl<TResolution>(TCommunity community, List<TResolution> list)
            where TResolution : TIResolution;
    }
}
