using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;

namespace DiceIoC
{
    public class Container
    {
        private IDictionary<RegistrationKey, Func<Container, object>> factories;
        private readonly IDictionary<Type, List<Func<Container, object>>> resolveAllFactories = new Dictionary<Type, List<Func<Container, object>>>();

        private readonly ICatalog catalog;
        private readonly object factoriesLock = new object();

        internal Container(ICatalog catalog)
        {
            this.catalog = catalog;
            GetFactories();
        }

        public T Resolve<T>(string name)
        {
            T resolved;
            if (!TryResolve(name, out resolved))
            {
                throw new ArgumentException(string.Format("The Name/Type {0}/{1} is not registered", name,
                                                          typeof (T).Name));
            }
            return resolved;
        }

        public T Resolve<T>()
        {
            return Resolve<T>(null);
        }

        public bool TryResolve<T>(string name, out T resolved)
        {
            resolved = default(T);
            object result;
            bool succeeded = TryResolve(RegistrationKey.For<T>(name), out result);
            if (succeeded)
            {
                resolved = (T) result;
            }
            return succeeded;
        }

        public bool TryResolve<T>(out T resolved)
        {
            return TryResolve(null, out resolved);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            List<Func<Container, object>> knownFactories;

            lock (factoriesLock)
            {
                if (!resolveAllFactories.TryGetValue(typeof (T), out knownFactories))
                {
                    knownFactories =
                        factories.Where(kvp => kvp.Key.Type == typeof (T)).Select(kvp => kvp.Value)
                        .Concat(GetFactories(typeof(T)))
                        .ToList();
                    resolveAllFactories[typeof (T)] = knownFactories;
                }
            }
            return knownFactories.Select(f => (T)f(this));
        }

        private bool TryResolve(RegistrationKey key, out object result)
        {
            Func<Container, object> factory;
            lock (factoriesLock)
            {
                bool found = factories.TryGetValue(key, out factory);
                if (!found)
                {
                    factory = GetFactory(key);
                    if (factory != null)
                    {
                        factories[key] = factory;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
            }
            result = factory(this);
            return true;
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
            var optimized = (Expression<Func<Container, object>>) visitor.Visit(expression);
            return optimized.Compile();
        }

        private Func<Container, object> GetFactory(RegistrationKey key)
        {
            var factoryExpression = catalog.GetFactoryExpression(key);
            if (factoryExpression != null)
            {
                return Compile(factoryExpression);
            }
            return null;
        }

        private IEnumerable<Func<Container, object>> GetFactories(Type serviceType)
        {
            return catalog.GetFactoryExpressions(serviceType).Select(Compile);
        }
    }
}
