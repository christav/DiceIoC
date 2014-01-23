using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    public class OpenGenericCatalog : CatalogBase, ICatalog
    {
        private struct FactoryEntry
        {
            public Type RegisteredType;
            public Expression<Func<Container, object>> FactoryExpression;

            public List<
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
            throw new NotImplementedException();
        }

        private IEnumerable<FactoryEntry> Get(RegistrationKey key)
        {
            return factories.Get(key, new List<FactoryEntry>());
        }

        private Expression<Func<Container, object>> SelectFactory(Type targetType,
            IEnumerable<FactoryEntry> possibilities)
        {
            Type[] targetTypeArgs = targetType.GetGenericArguments();
            foreach (var possibility in possibilities)
            {
                bool isMatch = possibility.RegisteredType.GetGenericArguments()
                    .Select((p, i) => new {Index = i, ParameterType = p})
                    .All(n => n.ParameterType == targetTypeArgs[n.Index] ||
                              GenericMarkers.IsGenericMarkerType(n.ParameterType, n.Index));

                if (isMatch)
                {
                    return ApplyModifiers(possibility.FactoryExpression, possibility.Modifiers);
                }
            }
            return null;
        }
    }
}
