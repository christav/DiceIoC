using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;

namespace DiceIoC.Containers
{
    class ContainerFactories
    {
        private IDictionary<RegistrationKey, Func<IContainer, object>> factories;
        private readonly IDictionary<Type, List<Func<IContainer, object>>> resolveAllFactories = new Dictionary<Type, List<Func<IContainer, object>>>();
        private readonly ICatalog catalog;
        private readonly object factoriesLock = new object();

        internal ContainerFactories(ICatalog catalog)
        {
            this.catalog = catalog;
            GetFactories();
        }

        internal bool TryGetFactory(RegistrationKey key, out Func<IContainer, object> factory)
        {
            lock (factoriesLock)
            {
                bool found = factories.TryGetValue(key, out factory);
                if (found)
                {
                    return true;
                }

                var factoryExpression = catalog.GetFactoryExpression(key);
                if (factoryExpression == null)
                {
                    return false;
                }

                factory = Compile(factoryExpression);
                factories[key] = factory;
                return true;
            }
        }

        internal IEnumerable<Func<IContainer, object>> GetAllFactories(Type serviceType)
        {
            List<Func<IContainer, object>> knownFactories;
            lock (factoriesLock)
            {
                if (!resolveAllFactories.TryGetValue(serviceType, out knownFactories))
                {
                    knownFactories =
                        factories.Where(kvp => kvp.Key.Type == serviceType).Select(kvp => kvp.Value)
                            .Concat(catalog.GetFactoryExpressions(serviceType).Select(Compile))
                            .ToList();
                    resolveAllFactories[serviceType] = knownFactories;
                }
            }
            return knownFactories;
        }

        private void GetFactories()
        {
            var expressions = catalog.GetFactoryExpressions();
            factories = expressions
                .ToDictionary(kvp => kvp.Key, kvp => Compile(kvp.Value));
        }

        private Func<IContainer, object> Compile(Expression<Func<IContainer, object>> expression)
        {
            var visitor = new ResolveCallInliningVisitor(catalog);
            var optimized = (Expression<Func<IContainer, object>>)visitor.Visit(expression);
            return optimized.Compile();
        }
    }
}