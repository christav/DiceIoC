using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DiceIoC.Catalogs
{
    public class GenericTypeRewritingVisitor : ExpressionVisitor
    {
        private readonly GenericMarkerConverter typeConverter;

        public GenericTypeRewritingVisitor(params Type[] substitutionTypes)
        {
            typeConverter = new GenericMarkerConverter(substitutionTypes);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (!(GenericMarkers.IsMarkedGeneric(node.Type)))
            {
                return base.VisitNew(node);
            }

            Type newTypeToConstruct = typeConverter.OpenToClosed(node.Type);
            ConstructorInfo newConstructor = typeConverter.OpenToClosed(node.Constructor, newTypeToConstruct);
            return Expression.New(newConstructor, node.Arguments.Select(n => Visit(node)));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Type[] funcArgs = typeof (T).GetGenericArguments();

            if (!funcArgs.Any(GenericMarkers.IsMarkedGeneric))
            {
                return base.VisitLambda(node);
            }

            var result = Expression.Lambda(
                Visit(node.Body), 
                node.Parameters.Select(p =>(ParameterExpression) Visit(p)));
            return result;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (GenericMarkers.IsMarkedGeneric(node.Type))
            {
                return Expression.Parameter(typeConverter.OpenToClosed(node.Type), node.Name);
            }
            return base.VisitParameter(node);
        }
    }
}
