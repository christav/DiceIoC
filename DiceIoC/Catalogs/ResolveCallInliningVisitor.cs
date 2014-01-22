using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DiceIoC.Catalogs
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
        private readonly ICatalog catalog;

        public ResolveCallInliningVisitor(ICatalog catalog)
        {
            this.catalog = catalog;
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
            var typeToResolve = node.Method.GetGenericArguments()[0];
            return GetOptimizedResolveExpression(typeToResolve, null, containerParam);
        }

        private Expression ReplaceResolveWithName(MethodCallExpression node)
        {
            var containerParam = node.Object;
            var nameExpression = node.Arguments[0];
            var typeToResolve = node.Method.GetGenericArguments()[0];

            if (nameExpression.NodeType == ExpressionType.Constant)
            {
                var name = (string)((ConstantExpression) nameExpression).Value;
                return GetOptimizedResolveExpression(typeToResolve, name, containerParam);
            }
            // If the name's not a constant, we can't optimize, but we can still
            // run the original expression.
            return node;
        }

        private Expression GetOptimizedResolveExpression(Type typeToResolve, string name, Expression containerParam)
        {
            Expression innerExpression = catalog.GetFactoryExpression(new RegistrationKey(typeToResolve, name));
            if (innerExpression == null)
            {
                throw new KeyNotFoundException(string.Format("The type {0} name {1} is not registered in the catalog",
                    typeToResolve.Name, name));
            }

            Expression optimizedFactory = Visit(innerExpression);
            
            return Expression.Convert(Expression.Invoke(optimizedFactory, containerParam), typeToResolve);
        }
    }
}
