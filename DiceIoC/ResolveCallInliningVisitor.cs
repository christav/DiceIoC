using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DiceIoC
{
    /// <summary>
    /// An expression visitor that looks for invocations of
    /// <see cref="Container.Resolve{T}(string)" /> or
    /// <see cref="Container.Resolve{T}()" /> and replaces them
    /// with the expression that the container would use to resolve
    /// that, with the goal of eliminating dictionary lookups at
    /// resolve time.
    /// </summary>
    public class ResolveCallInliningVisitor : ExpressionVisitor
    {
        private readonly Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>>  factories;

        public ResolveCallInliningVisitor(Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>> factories)
        {
            this.factories = factories;
        }

        private static readonly MethodInfo resolveWithNameMethod =
            typeof (Container).GetMethods()
                .Where(m => m.Name == "Resolve" && m.GetParameters().Length == 1)
                .Select(m => m.GetGenericMethodDefinition())
                .First();

        private static readonly MethodInfo resolveDefaultMethod =
            typeof (Container).GetMethods()
                .Where(m => m.Name == "Resolve" && m.GetParameters().Length == 0)
                .Select(m => m.GetGenericMethodDefinition())
                .First();

        private static bool IsResolveDefaultMethod(MethodInfo m)
        {
            return m.IsGenericMethod &&
                   m.GetGenericMethodDefinition() == resolveDefaultMethod;
        }

        private static bool IsResolveWithNameMethod(MethodInfo m)
        {
            return m.IsGenericMethod &&
                   m.GetGenericMethodDefinition() == resolveWithNameMethod;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (IsResolveDefaultMethod(node.Method)) return ReplaceResolveDefault(node);
            if (IsResolveWithNameMethod(node.Method)) return ReplaceResolveWithName(node);
            return base.VisitMethodCall(node);
        }

        private Expression ReplaceResolveDefault(MethodCallExpression node)
        {
            var containerParam = node.Object;
            var nameExpression = Expression.Constant(null, typeof (string));
            var typeToResolve = node.Method.GetGenericArguments()[0];
            var typeExpression = Expression.Constant(typeToResolve, typeof (Type));

            Expression actualFactory =
                new ResolveCallInliningVisitor(factories).Visit(factories[new RegistrationKey(null, typeToResolve)]);

            var cast = Expression.Convert(
                Expression.Invoke(actualFactory, containerParam, nameExpression, typeExpression), typeToResolve);

            return cast;
        }

        private Expression ReplaceResolveWithName(MethodCallExpression node)
        {
            var containerParam = node.Object;
            var nameExpression = node.Arguments[0];
            var typeToResolve = node.Method.GetGenericArguments()[0];
            var typeExpression = Expression.Constant(typeToResolve, typeof(Type));

            // We need to compile and execute the name expression to get the actual value out
            string name;

            if (nameExpression.NodeType == ExpressionType.Constant)
            {
                name = (string)((ConstantExpression) nameExpression).Value;
            }
            else
            {
                throw new InvalidOperationException("Name expression must be constant");
            }

            Expression actualFactory =
                new ResolveCallInliningVisitor(factories).Visit(factories[new RegistrationKey(name, typeToResolve)]);

            var cast = Expression.Convert(
                Expression.Invoke(actualFactory, containerParam, nameExpression, typeExpression), typeToResolve);

            return cast;
        }
    }
}
