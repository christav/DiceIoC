using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;

namespace DiceIoC.Containers
{
    class ContainerFactories
    {
        private IDictionary<RegistrationKey, Func<Container, object>> factories;
        private readonly IDictionary<Type, List<Func<Container, object>>> resolveAllFactories = new Dictionary<Type, List<Func<Container, object>>>();
        private readonly ICatalog catalog;
        private readonly object factoriesLock = new object();

        internal ContainerFactories(ICatalog catalog)
        {
            this.catalog = catalog;
            GetFactories();
        }

        internal bool TryGetFactory(RegistrationKey key, out Func<Container, object> factory)
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

        internal IEnumerable<Func<Container, object>> GetAllFactories(Type serviceType)
        {
            List<Func<Container, object>> knownFactories;
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

        private Func<Container, object> Compile(Expression<Func<Container, object>> expression)
        {
            var visitor = new ResolveCallInliningVisitor(catalog);
            var optimized = (Expression<Func<Container, object>>)visitor.Visit(expression);
            return optimized.Compile();
        }
    }
}