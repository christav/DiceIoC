using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
    public class OptimizingVisitor : ExpressionVisitor
    {
        private Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>>  factories;

        public OptimizingVisitor(Dictionary<RegistrationKey, Expression<Func<Container, string, Type, object>>> factories)
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

        private static bool IsResolveMethod(MethodInfo m)
        {
            return m.IsGenericMethod &&
                   m.GetGenericMethodDefinition() == resolveWithNameMethod ||
                   m.GetGenericMethodDefinition() == resolveDefaultMethod;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!IsResolveMethod(node.Method)) return base.VisitMethodCall(node);

            // TODO: Implement transformation
        }
    }
}
