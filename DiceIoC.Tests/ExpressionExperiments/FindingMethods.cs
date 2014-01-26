using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.ExpressionExperiments
{
    public class FindingMethods
    {
        private class OverloadedStaticMethods
        {
            public static void Method1(int arg)
            {
            }

            public static void Method1(string arg)
            {
                
            }

            public static void Method2<T>(T arg)
            {
                
            }

            public static void Method2<TArg0, TArg1>(TArg0 arg0, TArg1 arg1)
            {
                
            }
        }

        [Fact]
        public void CanMatchOverloadedMethods()
        {
            var methods = (from m in typeof (OverloadedStaticMethods).GetMethods()
                where m.Name == "Method1"
                select m).ToList();
            methods.Count.Should().Be(2);
        }

        [Fact]
        public void OverloadsOfGenericMethodsReturnGenericTypeDefinitions()
        {
            var methods = (from m in typeof (OverloadedStaticMethods).GetMethods()
                where m.Name == "Method2"
                select m).ToList();

            methods[0].IsGenericMethodDefinition.Should().BeTrue();
            methods[1].IsGenericMethodDefinition.Should().BeTrue();
        }
    }
}
