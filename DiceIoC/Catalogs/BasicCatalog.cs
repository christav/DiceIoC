using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Utils;

namespace DiceIoC.Catalogs
{
    public class BasicCatalog : CatalogBase, ICatalog
    {
        private readonly Dictionary<RegistrationKey, Expression<Func<IContainer, object>>> factories =
            new Dictionary<RegistrationKey, Expression<Func<IContainer, object>>>();

        // Registration API

        public override IRegistrar Register(Type serviceType, string name,
            Expression<Func<IContainer, object>> factoryExpression,
            params Func<
                Expression<Func<IContainer, object>>,
                Expression<Func<IContainer, object>>
            >[]  modifiers)
        {
            var key = new RegistrationKey(serviceType, name);
            if (!GenericMarkers.IsMarkedGeneric(key.Type))
            {
                factories[key] = ApplyModifiers(factoryExpression, modifiers);
            }
            return this;
        }

        public IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<IContainer, object>>>> GetFactoryExpressions()
        {
            return factories;
        }

        public IEnumerable<Expression<Func<IContainer, object>>> GetFactoryExpressions(Type serviceType)
        {
            // this catalog does not return on-demand factory expressions
            return Enumerable.Empty<Expression<Func<IContainer, object>>>();
        }

        /// <summary>
        /// Get factory expression for given key.
        /// </summary>
        /// <param name="key">Key to look up</param>
        /// <returns>The expression or null if key is not registered.</returns>
        public Expression<Func<IContainer, object>> GetFactoryExpression(RegistrationKey key)
        {
            return factories.Get(key);
        }
    }
}
