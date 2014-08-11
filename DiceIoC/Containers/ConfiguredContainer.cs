using System;
using System.Collections.Generic;
using DiceIoC.Catalogs;
using DiceIoC.Lifetimes;

namespace DiceIoC.Containers
{
    internal class ConfiguredContainer : Container
    {
        private readonly ContainerFactories factories;
        private readonly IScopedLifetime scope;

        internal ConfiguredContainer(ICatalog catalog)
        {
            factories = new ContainerFactories(catalog);
        }

        private ConfiguredContainer(ConfiguredContainer outer, IScopedLifetime scope)
        {
            factories = outer.factories;
            this.scope = scope;
        }

        public override object Resolve(Type serviceType, string name)
        {
            return new ResolveTimeContainer(factories, scope).Resolve(serviceType, name);
        }

        public override bool TryResolve(Type serviceType, string name, out object resolved)
        {
            return new ResolveTimeContainer(factories, scope).TryResolve(serviceType, name, out resolved);
        }

        public override IEnumerable<object> ResolveAll(Type serviceType)
        {
            return new ResolveTimeContainer(factories, scope).ResolveAll(serviceType);
        }

        public override IContainer InScope(IScopedLifetime scope)
        {
            return new ConfiguredContainer(this, scope);
        }

        public override IDictionary<int, object> PerResolveObjects { get { return null; } }

        public override IScopedLifetime CurrentScope { get { return scope; } }
    }
}
