using System;

namespace DiceIoC.Lifetimes
{
    /// <summary>
    /// Core interface for simplified implementation of lifetimes.
    /// Not required, but makes the job easier (less expression munging
    /// required).
    /// </summary>
    public interface ILifetime
    {
        /// <summary>
        /// Enter the lifetime - typically used to acquire a lock.
        /// </summary>
        /// <returns>An object that, when disposed, leaves the lifetime
        /// (typically by releasing the lock).</returns>
        IDisposable Enter();

        /// <summary>
        /// Retrieve the value from this lifetime object.
        /// </summary>
        /// <param name="c">Container being used to resolve.</param>
        /// <returns>The stored object, or null if there's no value yet.</returns>
        object GetValue(Container c);

        /// <summary>
        /// Set a new value for this lifetime object.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="c">Container being used to resolve.</param>
        /// <returns>The value of the <paramref name="value"/> parameter.</returns>
        object SetValue(object value, Container c);
    }
}
