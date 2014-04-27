using System.Collections.Generic;

namespace DiceIoC
{
    public interface IContainer
    {
        T Resolve<T>(string name);
        T Resolve<T>();
        bool TryResolve<T>(string name, out T resolved);
        bool TryResolve<T>(out T resolved);
        IEnumerable<T> ResolveAll<T>();

        IDictionary<int, object> PerResolveObjects { get; } 
    }
}
