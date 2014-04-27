using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;

namespace DiceIoC
{
    public class Container : IContainer
    {
        private IDictionary<RegistrationKey, Func<IContainer, object>> factories;
        private readonly IDictionary<Type, List<Func<IContainer, object>>> resolveAllFactories = new Dictionary<Type, List<Func<IContainer, object>>>();

        private readonly ICatalog catalog;
        private readonly object factoriesLock = new object();

        internal Container(ICatalog catalog)
        {
            this.catalog = catalog;
            GetFactories();
        }

        private class ResolveTimeContainer : IContainer
        {
            private readonly Container outerContainer;

            public ResolveTimeContainer(Container outerContainer)
            {
                this.outerContainer = outerContainer;
            }

            public T Resolve<T>(string name)
            {
                T resolved;
                if (!TryResolve(name, out resolved))
                {
                    throw new ArgumentException(string.Format("The Name/Type {0}/{1} is not registered", name,
                                                              typeof(T).Name));
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
                    resolved = (T)result;
                }
                return succeeded;
            }

            public bool TryResolve<T>(out T resolved)
            {
                return TryResolve(null, out resolved);
            }

            public IEnumerable<T> ResolveAll<T>()
            {
                List<Func<IContainer, object>> knownFactories;

                lock (outerContainer.factoriesLock)
                {
                    if (!outerContainer.resolveAllFactories.TryGetValue(typeof(T), out knownFactories))
                    {
                        knownFactories =
                            outerContainer.factories.Where(kvp => kvp.Key.Type == typeof(T)).Select(kvp => kvp.Value)
                            .Concat(GetFactories(typeof(T)))
                            .ToList();
                        outerContainer.resolveAllFactories[typeof(T)] = knownFactories;
                    }
                }
                return knownFactories.Select(f => (T)f(this));
            }

            private bool TryResolve(RegistrationKey key, out object result)
            {
                Func<IContainer, object> factory;
                lock (outerContainer.factoriesLock)
                {
                    bool found = outerContainer.factories.TryGetValue(key, out factory);
                    if (!found)
                    {
                        factory = GetFactory(key);
                        if (factory != null)
                        {
                            outerContainer.factories[key] = factory;
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

            private Func<IContainer, object> Compile(Expression<Func<IContainer, object>> expression)
            {
                var visitor = new ResolveCallInliningVisitor(outerContainer.catalog);
                var optimized = (Expression<Func<IContainer, object>>)visitor.Visit(expression);
                return optimized.Compile();
            }

            private Func<IContainer, object> GetFactory(RegistrationKey key)
            {
                var factoryExpression = outerContainer.catalog.GetFactoryExpression(key);
                if (factoryExpression != null)
                {
                    return Compile(factoryExpression);
                }
                return null;
            }

            private IEnumerable<Func<IContainer, object>> GetFactories(Type serviceType)
            {
                return outerContainer.catalog.GetFactoryExpressions(serviceType).Select(Compile);
            }
        }

        public T Resolve<T>(string name)
        {
            return new ResolveTimeContainer(this).Resolve<T>(name);
        }

        public T Resolve<T>()
        {
            return Resolve<T>(null);
        }

        public bool TryResolve<T>(string name, out T resolved)
        {
            return new ResolveTimeContainer(this).TryResolve<T>(name, out resolved);
        }

        public bool TryResolve<T>(out T resolved)
        {
            return TryResolve(null, out resolved);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return new ResolveTimeContainer(this).ResolveAll<T>();
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
            var optimized = (Expression<Func<IContainer, object>>) visitor.Visit(expression);
            return optimized.Compile();
        }
    }
}
