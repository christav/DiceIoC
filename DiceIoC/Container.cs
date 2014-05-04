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
        private readonly IScopedLifetime scope;

        internal Container(ICatalog catalog)
        {
            this.catalog = catalog;
            GetFactories();
        }

        private Container(Container outer, IScopedLifetime scope)
        {
            factories = outer.factories;
            resolveAllFactories = outer.resolveAllFactories;
            catalog = outer.catalog;
            factoriesLock = outer.factoriesLock;
            this.scope = scope;
        }

        private class ResolveTimeContainer : IContainer
        {
            private readonly Container outerContainer;
            private readonly Dictionary<int, object> perResolveObjects = new Dictionary<int, object>();
 
            public ResolveTimeContainer(Container outerContainer)
            {
                this.outerContainer = outerContainer;
            }

            public object Resolve(Type serviceType, string name)
            {
                object resolved;
                if (!TryResolve(serviceType, name, out resolved))
                {
                    throw new ArgumentException(string.Format("The Name/Type {0}/{1} is not registered", name,
                                                              serviceType.Name));
                }
                return resolved;
            }

            public T Resolve<T>(string name)
            {
                return (T) Resolve(typeof (T), name);
            }

            public T Resolve<T>()
            {
                return Resolve<T>(null);
            }

            public object Resolve(Type serviceType)
            {
                return Resolve(serviceType, null);
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

            public bool TryResolve(Type serviceType, string name, out object resolved)
            {
                return TryResolve(new RegistrationKey(serviceType, name), out resolved);
            }

            public bool TryResolve<T>(out T resolved)
            {
                return TryResolve(null, out resolved);
            }

            public bool TryResolve(Type serviceType, out object resolved)
            {
                return TryResolve(new RegistrationKey(serviceType, null), out resolved);
            }

            public IEnumerable<T> ResolveAll<T>()
            {
                return ResolveAll(typeof (T)).Cast<T>();
            }

            public IEnumerable<object> ResolveAll(Type serviceType)
            {
                return outerContainer.GetAllFactories(serviceType).Select(f => f(this));
            }

            public IContainer InScope(IScopedLifetime scope)
            {
                throw new InvalidOperationException("Cannot change scope during a resolve");
            }

            public IDictionary<int, object> PerResolveObjects { get { return perResolveObjects; } }

            public IScopedLifetime CurrentScope { get { return outerContainer.CurrentScope; } }

            private bool TryResolve(RegistrationKey key, out object result)
            {
                Func<IContainer, object> factory;
                if (outerContainer.TryGetFactory(key, out factory))
                {
                    result = factory(this);
                    return true;
                }

                result = null;
                return false;
            }
        }

        public T Resolve<T>(string name)
        {
            return new ResolveTimeContainer(this).Resolve<T>(name);
        }

        public T Resolve<T>()
        {
            return new ResolveTimeContainer(this).Resolve<T>();
        }

        public bool TryResolve<T>(string name, out T resolved)
        {
            return new ResolveTimeContainer(this).TryResolve(name, out resolved);
        }

        public bool TryResolve<T>(out T resolved)
        {
            return new ResolveTimeContainer(this).TryResolve(out resolved);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return new ResolveTimeContainer(this).ResolveAll<T>();
        }

        public object Resolve(Type serviceType, string name)
        {
            return new ResolveTimeContainer(this).Resolve(serviceType, name);
        }

        public object Resolve(Type serviceType)
        {
            return new ResolveTimeContainer(this).Resolve(serviceType);
        }

        public bool TryResolve(Type serviceType, string name, out object resolved)
        {
            return new ResolveTimeContainer(this).TryResolve(serviceType, name, out resolved);
        }

        public bool TryResolve(Type serviceType, out object resolved)
        {
            return new ResolveTimeContainer(this).TryResolve(serviceType, out resolved);
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            return new ResolveTimeContainer(this).ResolveAll(serviceType);
        }

        public IContainer InScope(IScopedLifetime scope)
        {
            return new Container(this, scope);
        }

        public IDictionary<int, object> PerResolveObjects { get { return null; } }

        public IScopedLifetime CurrentScope { get { return scope; } }

        private void GetFactories()
        {
            var expressions = catalog.GetFactoryExpressions();
            factories = expressions
                .ToDictionary(kvp => kvp.Key, kvp => Compile(kvp.Value));
        }

        private bool TryGetFactory(RegistrationKey key, out Func<IContainer, object> factory)
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

        private Func<IContainer, object> Compile(Expression<Func<IContainer, object>> expression)
        {
            var visitor = new ResolveCallInliningVisitor(catalog);
            var optimized = (Expression<Func<IContainer, object>>) visitor.Visit(expression);
            return optimized.Compile();
        }

        private IEnumerable<Func<IContainer, object>> GetAllFactories(Type serviceType)
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

        
    }
}
