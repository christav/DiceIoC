using System;
using System.Collections.Generic;
using System.Linq;
using DiceIoC.Catalogs;
using DiceIoC.Lifetimes;

namespace DiceIoC.Containers
{
    internal class ResolveTimeContainer : Container
    {
        private readonly ContainerFactories factories;
        private readonly IScopedLifetime scope;
        private readonly Dictionary<int, object> perResolveObjects = new Dictionary<int, object>();
 
        public ResolveTimeContainer(ContainerFactories factories, IScopedLifetime scope)
        {
            this.scope = scope;
            this.factories = factories;
        }

        public override object Resolve(Type serviceType, string name)
        {
            object resolved;
            if (!TryResolve(serviceType, name, out resolved))
            {
                throw new ArgumentException(String.Format("The Name/Type {0}/{1} is not registered", name,
                    serviceType.Name));
            }
            return resolved;
        }

        public override bool TryResolve(Type serviceType, string name, out object resolved)
        {
            return TryResolve(new RegistrationKey(serviceType, name), out resolved);
        }

        public override IEnumerable<object> ResolveAll(Type serviceType)
        {
            return factories.GetAllFactories(serviceType).Select(f => f(this));
        }

        public override IContainer InScope(IScopedLifetime scope)
        {
            throw new InvalidOperationException("Cannot change scope during a resolve");
        }

        public override IDictionary<int, object> PerResolveObjects { get { return perResolveObjects; } }

        public override IScopedLifetime CurrentScope { get { return scope; } }

        private bool TryResolve(RegistrationKey key, out object result)
        {
            Func<IContainer, object> factory;
            if (factories.TryGetFactory(key, out factory))
            {
                result = factory(this);
                return true;
            }

            result = null;
            return false;
        }
    }
}