using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace AYip.Foundations
{
    /// <summary>
    /// Responsible for counting the reference for the object, then manually deal with the release method.
    /// </summary>
    public abstract class RefCounter<T> : DisposableBase
    {
        /// <summary>
        /// The resource to check for the reference count.
        /// </summary>
        protected T instance;
        
        /// <summary>
        /// Used to cancel the ref count checking.
        /// </summary>
        protected CancellationTokenSource _cts;
        
        /// <summary>
        /// Since RefCounter can be used on multiple threads, it's necessary to lock the cts in case of race condition.
        /// </summary>
        private readonly object _ctsLock = new();
        
        protected RefCounter(T instance, int lifeTime = 1)
        {
            this.instance = instance;
            RefCount = 0;
            Lifetime = lifeTime;
        }

        /// <summary>
        /// The current reference count.
        /// </summary>
        public int RefCount { get; private set; }
        
        /// <summary>
        /// It determines how many frames it will wait for resource release checking.
        /// </summary>
        public int Lifetime { get; }
        
        /// <summary>
        /// Register to use the reference, please invoke the Complete action after use.
        /// </summary>
        /// <returns></returns>
        public (T instance, UnityAction CompleteRefUse) RegisterToUse()
        {
            RefCount++;
            return (instance, CompleteRefUse: CompleteTask);
        }
        
        /// <summary>
        /// Directly complete the usage of this reference.
        /// </summary>
        public void CompleteTask()
        {
            lock (_ctsLock)
            {
                RefCount--;
                _cts?.Cancel();
            }
            ReleaseResourceCheck();
        }

        protected virtual async Task ReleaseResourceCheck()
        {
            var frame = 0;

            try
            {
                while (frame++ >= Lifetime)
                {
                    await Task.Yield();

                    lock (_ctsLock)
                    {
                        if (_cts is { IsCancellationRequested: true })
                            throw new OperationCanceledException(_cts.Token);
                    }
                }

                lock (_ctsLock)
                {
                    if (RefCount > 0) return;
                    ReleaseInstance();
                    instance = default;
                    Dispose();
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                lock (_ctsLock)
                {
                    _cts?.Dispose();
                    _cts = null;
                }
            }
        }

        /// <summary>
        /// Handle your release method.
        /// </summary>
        protected abstract void ReleaseInstance();
    }
}