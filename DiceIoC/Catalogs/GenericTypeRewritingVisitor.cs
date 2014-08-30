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
            return Expression.New(newConstructor, node.Arguments.Select(Visit));
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert &&
                GenericMarkers.IsMarkedGeneric(node.Type))
            {
                return Expression.Convert(Visit(node.Operand), typeConverter.OpenToClosed(node.Type));
            }
            return base.VisitUnary(node);
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            bool requiresRewrite = ObjectIsMarkedGeneric(node.Object) ||
                MethodIsOnMarkedGenericType(node.Method) ||
                MethodIsMarkedGeneric(node.Method) ||
                ArgumentsAreMarkedGeneric(node.Arguments);

            if (!requiresRewrite)
            {
                return base.VisitMethodCall(node);
            }
            return RewriteCall(node);
        }

        private Expression RewriteCall(MethodCallExpression expression)
        {
            MethodInfo method = expression.Method;
            Expression instanceExpression;
            Type declaringType;
            BindingFlags bindingFlags;

            if (IsCallToStaticMethod(expression))
            {
                instanceExpression = null;
                declaringType = typeConverter.OpenToClosed(method.DeclaringType);
                bindingFlags = BindingFlags.Static;
            }
            else
            {
                instanceExpression = Visit(expression.Object);
                declaringType = instanceExpression.Type;
                bindingFlags = BindingFlags.Instance;
            }

            bindingFlags |= method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;

            var argExpressions = expression.Arguments.Select(Visit).ToList();
            var candidateMethods = declaringType.GetMethods(bindingFlags)
                .Where(m => m.Name == method.Name && m.GetParameters().Length == argExpressions.Count);

            if (method.IsGenericMethod)
            {
                var typeArgs = method.GetGenericArguments().Select(typeConverter.OpenToClosed).ToArray();
                candidateMethods = candidateMethods
                    .Where(m => m.IsGenericMethodDefinition && m.GetGenericArguments().Length == typeArgs.Length)
                    .Select(m => m.MakeGenericMethod(typeArgs));
            }

            var argTypes = argExpressions.Select(e => e.Type).ToArray();
            var newMethod =
                candidateMethods.First(m => argTypes.SequenceEqual(m.GetParameters().Select(p => p.ParameterType)));

            return Expression.Call(instanceExpression, newMethod, argExpressions);
        }

        private static bool IsCallToStaticMethod(MethodCallExpression expression)
        {
            return expression.Object == null;
        }

        private static bool MethodIsOnMarkedGenericType(MethodInfo method)
        {
            return GenericMarkers.IsMarkedGeneric(method.DeclaringType);
        }

        private static bool ObjectIsMarkedGeneric(Expression o)
        {
            return o != null && GenericMarkers.IsGenericMarkerType(o.Type);
        }

        private static bool MethodIsMarkedGeneric(MethodInfo method)
        {
            return method.IsGenericMethod && method.GetGenericArguments().Any(GenericMarkers.IsMarkedGeneric);
        }

        private static bool ArgumentsAreMarkedGeneric(IEnumerable<Expression> args)
        {
            return args.Any(e => GenericMarkers.IsMarkedGeneric(e.Type));
        }
    }
}
