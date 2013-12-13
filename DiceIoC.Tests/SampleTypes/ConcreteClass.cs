using System;

namespace DiceIoC.Tests.SampleTypes
{
    class ConcreteClass : IDisposable
    {
        public bool Disposed = false;

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
