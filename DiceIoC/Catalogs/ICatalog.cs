using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DiceIoC.Containers;

namespace DiceIoC.Catalogs
{
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
        IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<IContainer, object>>>>
            GetFactoryExpressions();

        /// <summary>
        /// Get the factory expression for a given key.
        /// </summary>
        /// <param name="key">The key to the type/name pair desired.</param>
        /// <returns>The factory expression, or null if no factory can be found or created.</returns>
        Expression<Func<IContainer, object>> GetFactoryExpression(RegistrationKey key);

        /// <summary>
        /// Get all the factory expressions for a known service type regardless of name.
        /// Used in calls to <see cref="ConfiguredContainer.ResolveAll{T}"/>
        /// </summary>
        /// <param name="serviceType">Type to get factories for.</param>
        /// <returns>The set of factories.</returns>
        IEnumerable<Expression<Func<IContainer, object>>> GetFactoryExpressions(Type serviceType);
    }
}
