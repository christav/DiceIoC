using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DiceIoC.Tests.Bootstrap
{
    public class CanRun
    {
        [Fact]
        public void CanRunThisTest()
        {
            true.Should().BeTrue();
        }
    }
}
