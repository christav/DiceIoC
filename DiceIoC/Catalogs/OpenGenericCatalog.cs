using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DiceIoC.Catalogs
{
    public class OpenGenericCatalog : CatalogBase, ICatalog
    {
        private readonly Dictionary<RegistrationKey, List<Tuple<Type, Expression<Func<Container, object>>>>> factories =
            new Dictionary<RegistrationKey, List<Tuple<Type, Expression<Func<Container, object>>>>>();

        // Registration API

        public override IRegistrar Register(Type serviceType, string name,
            Expression<Func<Container, object>> factoryExpression,
            params Func<
                Expression<Func<Container, object>>,
                Expression<Func<Container, object>>
            >[] modifiers)
        {
            var key = new RegistrationKey(serviceType, name);
            if (key.IsValidOpenGenericRegistration)
            {
                var dictKey = new RegistrationKey(serviceType.GetGenericTypeDefinition(), name);

                if (!factories.ContainsKey(dictKey))
                {
                    factories[dictKey] = new List<Tuple<Type, Expression<Func<Container, object>>>>();
                }
                factories[dictKey].Add(Tuple.Create(serviceType, ApplyModifiers(factoryExpression, modifiers)));
            }
            return this;
        }

        /// <summary>
        /// This catalog only gives registrations on demand, not when asking for all.
        /// </summary>
        /// <returns>An empty dictionary.</returns>
        public IDictionary<RegistrationKey, Expression<Func<Container, object>>> GetFactoryExpressions()
        {
            return new Dictionary<RegistrationKey, Expression<Func<Container, object>>>();
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

        private IEnumerable<Tuple<Type, Expression<Func<Container, object>>>> Get(RegistrationKey key)
        {
            return factories.Get(key, new List<Tuple<Type, Expression<Func<Container, object>>>>());
        }

        private Expression<Func<Container, object>> SelectFactory(Type targetType,
            IEnumerable<Tuple<Type, Expression<Func<Container, object>>>> possibilities)
        {
            Type[] targetTypeArgs = targetType.GetGenericArguments();
            foreach (var possibility in possibilities)
            {
                bool isMatch = possibility.Item1.GetGenericArguments()
                    .Select((p, i) => new {Index = i, ParameterType = p})
                    .All(n => n.ParameterType == targetTypeArgs[n.Index] ||
                              RegistrationKey.IsGenericMarkerType(n.ParameterType, n.Index));

                if (isMatch)
                {
                    return possibility.Item2;
                }
            }
            return null;
        }
    }
}
