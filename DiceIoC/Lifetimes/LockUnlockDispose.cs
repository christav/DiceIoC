using System;
using System.Threading;

namespace DiceIoC.Lifetimes
{
    //
    // A class that takes a lock on construction and
    // releases the lock at dispose time.
    //
    sealed class LockUnlockDispose : IDisposable
    {
        private readonly object padlock;

        public LockUnlockDispose(object padlock)
        {
            this.padlock = padlock;
            Monitor.Enter(this.padlock);
        }

        public void Dispose()
        {
            Monitor.Exit(padlock);
        }
    }
}
