using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DiceIoC.Utils;

namespace DiceIoC.Catalogs
{
    public class OpenGenericCatalog : CatalogBase, ICatalog
    {
        private struct FactoryEntry
        {
            public readonly Type RegisteredType;
            public readonly Expression<Func<Container, object>> FactoryExpression;

            public readonly List<
                Func<
                    Expression<Func<Container, object>>,
                    Expression<Func<Container, object>>
                >
            > Modifiers;

            public FactoryEntry(
                Type registeredType, 
                Expression<Func<Container, object>> factoryExpression, 
                IEnumerable<Func<Expression<Func<Container, object>>, Expression<Func<Container, object>>>> modifiers)
            {
                RegisteredType = registeredType;
                FactoryExpression = factoryExpression;
                Modifiers = modifiers.ToList();
            }
        }

        private readonly Dictionary<RegistrationKey, List<FactoryEntry>> factories =
            new Dictionary<RegistrationKey, List<FactoryEntry>>();

        // Registration API

        public override IRegistrar Register(Type serviceType, string name,
            Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            if (GenericMarkers.IsValidMarkedGeneric(serviceType))
            {
                var dictKey = new RegistrationKey(serviceType.GetGenericTypeDefinition(), name);

                if (!factories.ContainsKey(dictKey))
                {
                    factories[dictKey] = new List<FactoryEntry>();
                }
                factories[dictKey].Add(new FactoryEntry(serviceType, factoryExpression, modifiers));
            }
            return this;
        }

        /// <summary>
        /// This catalog only gives registrations on demand, not when asking for all.
        /// </summary>
        /// <returns>An empty dictionary.</returns>
        public IEnumerable<KeyValuePair<RegistrationKey, Expression<Func<Container, object>>>> GetFactoryExpressions()
        {
            return Enumerable.Empty<KeyValuePair<RegistrationKey, Expression<Func<Container, object>>>>();
        }

        public Expression<Func<Container, object>> GetFactoryExpression(RegistrationKey key)
        {
            if (!key.Type.IsGenericType) return null;

            var dictKey = new RegistrationKey(key.Type.GetGenericTypeDefinition(), key.Name);
            var possibleFactories = Get(dictKey);
            var factoryExpression = SelectFactory(key.Type, possibleFactories);
            var visitor = new GenericTypeRewritingVisitor(key.Type.GetGenericArguments());
            return (Expression<Func<Container, object>>) (visitor.Visit(factoryExpression));
        }

        public IEnumerable<Expression<Func<Container, object>>> GetFactoryExpressions(Type serviceType)
        {
            if (!serviceType.IsGenericType)
            {
                yield break;
            }

            var genericType = serviceType.GetGenericTypeDefinition();
            foreach (var registration in factories.Where(kvp => kvp.Key.Type == genericType))
            {
                foreach (var entry in registration.Value)
                {
                    if (IsPossibleMatch(serviceType, entry.RegisteredType))
                    {
                        var visitor = new GenericTypeRewritingVisitor(serviceType.GetGenericArguments());
                        var genericFactory = ApplyModifiers(entry.FactoryExpression, entry.Modifiers);
                        yield return ((Expression<Func<Container, object>>)visitor.Visit(genericFactory));
                    }
                }
            }
        }

        private IEnumerable<FactoryEntry> Get(RegistrationKey key)
        {
            return factories.Get(key, new List<FactoryEntry>());
        }

        private Expression<Func<Container, object>> SelectFactory(Type targetType,
            IEnumerable<FactoryEntry> possibilities)
        {
            return possibilities
                .Where(p => IsPossibleMatch(targetType, p.RegisteredType))
                .Select(p => ApplyModifiers(p.FactoryExpression, p.Modifiers))
                .FirstOrDefault();
        }

        private static bool IsPossibleMatch(Type requestedType, Type possibleType)
        {
            Type[] requestedTypeArgs = requestedType.GetGenericArguments();
            return possibleType.GetGenericArguments()
                .Select((p, i) => new {Index = i, ParameterType = p})
                .All(n => n.ParameterType == requestedTypeArgs[n.Index] ||
                          GenericMarkers.IsGenericMarkerType(n.ParameterType, n.Index));
        }
    }
}
