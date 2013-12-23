using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DiceIoC
{
    /// <summary>
    /// A basic object that lets you manage the lifetime of disposable
    /// objects. The first time the container creates an object it'll
    /// be put in the container, but subsequent resolves will return
    /// the object directly.
    /// 
    /// If you call "Clear" or "Dispose" on the LifetimeContainer, all objects
    /// stored in it are disposed and the next time a resolve happens
    /// the object will be recreated.
    /// </summary>
    public sealed class LifetimeContainer : IDisposable
    {
        private readonly List<ContainedLifetime> containedObjects = new List<ContainedLifetime>();
        private readonly object padlock = new object();

        public void Dispose()
        {
            foreach (var o in containedObjects)
            {
                o.Dispose();
            }
            containedObjects.Clear();
        }

        public void Clear()
        {
            Dispose();
        }

        // An instance of this lifetime object manages
        // each of the contained objects
        private class ContainedLifetime : Lifetime, IDisposable
        {
            private object value;

            public ContainedLifetime(LifetimeContainer lifetimeContainer)
            {
                lifetimeContainer.Add(this);
            }

            public void Dispose()
            {
                if (value is IDisposable)
                {
                    ((IDisposable) value).Dispose();
                    value = null;
                }
            }

            public override object GetValue(Container c)
            {
                return value;
            }

            public override object SetValue(object value, Container c)
            {
                return this.value = value;
            }

            public new Func<Expression<Func<Container, object>>,
                Expression<Func<Container,object>>> LifetimeModifier
            {
                get { return base.LifetimeModifier; }
            }
        }

        private void Add(ContainedLifetime containedObject)
        {
            lock (padlock)
            {
                containedObjects.Add(containedObject);
            }
        }

        private Func<Expression<Func<Container, object>>,
            Expression<Func<Container, object>>> Lifetime
        {
            get
            {
                return new ContainedLifetime(this).LifetimeModifier;
            }
        }

        public static implicit operator
            Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>(
            LifetimeContainer lifetime)
        {
            return lifetime.Lifetime;
        }
    }
}
