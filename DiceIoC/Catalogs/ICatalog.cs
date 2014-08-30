using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DiceIoC.Containers;

namespace DiceIoC.Catalogs
{
    using FactoryExpression = Expression<Func<Container, object>>;

    /// <summary>
    /// Interface that <see cref="ConfiguredContainer"/> instances
    /// use to interface with a catalog.
    /// </summary>
    public interface ICatalog
    {
        /// <summary>
        /// Get the set of factory expressions registered in this catalog.
        /// </summary>
        /// <returns>Currently registered factories.</returns>
        IEnumerable<KeyValuePair<RegistrationKey, FactoryExpression>>
            GetFactoryExpressions();

        /// <summary>
        /// Get the factory expression for a given key.
        /// </summary>
        /// <param name="key">The key to the type/name pair desired.</param>
        /// <returns>The factory expression, or null if no factory can be found or created.</returns>
        FactoryExpression GetFactoryExpression(RegistrationKey key);

        /// <summary>
        /// Get all the factory expressions for a known service type regardless of name.
        /// Used in calls to <see cref="ConfiguredContainer.ResolveAll{T}"/>
        /// </summary>
        /// <param name="serviceType">Type to get factories for.</param>
        /// <returns>The set of factories.</returns>
        IEnumerable<FactoryExpression> GetFactoryExpressions(Type serviceType);
    }
}
