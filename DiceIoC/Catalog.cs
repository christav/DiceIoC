using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Catalogs;
using DiceIoC.Containers;
using DiceIoC.Registrations;
using DiceIoC.Utils;

namespace DiceIoC
{
    public class Catalog
    {
        private readonly Dictionary<RegistrationKey, IRegistration> concreteRegistrations =
            new Dictionary<RegistrationKey, IRegistration>();

        private readonly Dictionary<RegistrationKey, List<IRegistration>> genericRegistrations =
            new Dictionary<RegistrationKey, List<IRegistration>>();


        public Catalog Register(Type serviceType, string name, 
            Expression<Func<Container, object>> factoryExpression, 
            params Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>[] modifiers)
        {
            var key = new RegistrationKey(serviceType, name);
            var registration = RegistrationBuilder.CreateRegistration(serviceType, factoryExpression, modifiers);
            if (GenericMarkers.IsMarkedGeneric(serviceType))
            {
                key = new RegistrationKey(serviceType.GetGenericTypeDefinition(), name);
                List<IRegistration> currentRegistrations = genericRegistrations.Get(key, new List<IRegistration>());
                currentRegistrations.Add(registration);
                genericRegistrations[key] = currentRegistrations;
            }
            else
            {
                concreteRegistrations[key] = registration;
            }
            return this;
        }

        public Catalog Register<TService>(string name, Expression<Func<Container, TService>> factoryExpression, 
            params Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>[] modifiers)
        {
            return Register(typeof (TService), name, CastToObject(factoryExpression), modifiers);
        }

        public Catalog Register<TService>(Expression<Func<Container, TService>> factoryExpression, 
            params Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>[] modifiers)
        {
            return Register(typeof (TService), null, CastToObject(factoryExpression), modifiers);
        }

        public Container CreateContainer()
        {
            return new ConfiguredContainer(new CatalogImpl(this));
        }

        public Catalog With(Func<Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>> modiferFactory, Action<IRegistrar> registrations)
        {
            var registrar = new WithModifierRegistrar(modiferFactory);
            registrations(registrar);
            registrar.Register(this);
            return this;
        }

        private class CatalogImpl : ICatalog
        {
            private readonly Dictionary<RegistrationKey, IRegistration> concreteRegistrations =
                new Dictionary<RegistrationKey, IRegistration>();

            private readonly Dictionary<RegistrationKey, List<IRegistration>> genericRegistrations =
                new Dictionary<RegistrationKey, List<IRegistration>>();

            public CatalogImpl(Catalog outer)
            {
                concreteRegistrations = outer.concreteRegistrations;
                genericRegistrations = outer.genericRegistrations;
            }

            public IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<Container, object>>>> GetFactoryExpressions()
            {
                return concreteRegistrations
                    .Select(kvp => new KeyValuePair<RegistrationKey, Expression<Func<Container, object>>>(
                        kvp.Key, kvp.Value.GetFactory()));
            }

            public Expression<Func<Container, object>> GetFactoryExpression(RegistrationKey key)
            {
                return GetConcreteFactoryExpression(key) ?? GetGenericFactoryExpression(key);
            }

            public IEnumerable<Expression<Func<Container, object>>> GetFactoryExpressions(Type serviceType)
            {
                if (!serviceType.IsGenericType)
                {
                    return Enumerable.Empty<Expression<Func<Container, object>>>();
                }

                var genericType = serviceType.GetGenericTypeDefinition();
                return genericRegistrations
                    .Where(kvp => kvp.Key.Type == genericType)
                    .SelectMany(kvp => kvp.Value)
                    .Select(r => r.GetFactory(serviceType))
                    .Where(r => r != null);
            }

            private Expression<Func<Container, object>> GetConcreteFactoryExpression(RegistrationKey key)
            {
                var registration = concreteRegistrations.Get(key);
                Expression<Func<Container, object>> factory = null;
                if (registration != null)
                {
                    factory = registration.GetFactory();
                }
                return factory;
            }

            private Expression<Func<Container, object>> GetGenericFactoryExpression(RegistrationKey key)
            {
                if (!key.Type.IsGenericType) return null;

                var key2 = new RegistrationKey(key.Type.GetGenericTypeDefinition(), key.Name);
                return genericRegistrations.Get(key2, new List<IRegistration>())
                    .Select(r => r.GetFactory(key.Type)).FirstOrDefault(e => e != null);
            }
        }

        internal static Expression<Func<Container, object>> CastToObject<T>(
            Expression<Func<Container, T>> originalExpression)
        {
            var c = Expression.Parameter(typeof(Container), "c");

            var cast = Expression.Convert(
                Expression.Invoke(originalExpression, c), typeof(object));

            return Expression.Lambda<Func<Container, object>>(
                cast, c);
        }
    }
}
