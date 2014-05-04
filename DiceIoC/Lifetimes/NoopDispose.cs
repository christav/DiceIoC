using System;

namespace DiceIoC.Lifetimes
{
    //
    // Noop implementation of dispose.
    //
    class NoopDispose : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
