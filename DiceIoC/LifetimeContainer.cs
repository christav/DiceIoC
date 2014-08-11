using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DiceIoC.Lifetimes;

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
        private sealed class ContainedLifetime : SynchronizedLifetime, IDisposable
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

// ReSharper disable once UnusedMember.Local
            // Method is used via dyamically generated expression
// ReSharper disable once UnusedParameter.Local
            public object GetValue(Container c)
            {
                return value;
            }

// ReSharper disable once UnusedMember.Local
            // Method is used via dynamically generated expression
// ReSharper disable once UnusedParameter.Local
            public object SetValue(object newValue, Container c)
            {
                return (value = newValue);
            }
        }

        private void Add(ContainedLifetime containedObject)
        {
            lock (padlock)
            {
                containedObjects.Add(containedObject);
            }
        }

        public Func<Expression<Func<Container, object>>,
            Expression<Func<Container, object>>> Lifetime
        {
            get
            {
                return factory => Lifetimes.Lifetime.RewriteForLifetime(factory, new ContainedLifetime(this));
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
