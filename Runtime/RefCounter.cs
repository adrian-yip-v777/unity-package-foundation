using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace vz777.Foundations
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
        protected CancellationTokenSource cts;
        
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

        protected override void DisposeManagedResources()
        {
            cts?.Dispose();
            cts = null;
        }

        /// <summary>
        /// Register to use the reference, please invoke the Complete action after use.
        /// </summary>
        /// <returns></returns>
        public (T instance, Action CompleteRefUse) RegisterToUse()
        {
            RefCount++;
            return (instance, CompleteRefUse: CompleteTask);
        }
        
        /// <summary>
        /// Directly complete the usage of this reference.
        /// </summary>
        public void CompleteTask()
        {
            if (IsDisposed)
            {
                Debug.LogWarning("Completing a ref counter that has been disposed.");
                return;
            }
            
            RefCount--;
            
            // Reset the dispose timer.
            cts?.Cancel();
            cts = new CancellationTokenSource();
            
            StartDisposeTimer();
        }

        /// <summary>
        /// Start a timer for disposing this wrapper and releasing the target resource.
        /// </summary>
        protected virtual void StartDisposeTimer()
        {
            Task.Run(async () =>
            {
                var frameCount = 0;

                try
                {
                    while (frameCount++ < Lifetime)
                    {
                        await Task.Yield();

                        // Handle manually dispose case.
                        if (IsDisposed) return;
                    
                        // Handle cancel case.
                        if (cts is { IsCancellationRequested: true })
                            throw new OperationCanceledException(cts.Token);
                    }

                    // If there is ref count left, do nothing.
                    if (RefCount > 0) return;
                
                    // If there is no ref count left, dispose this wrapper.
                    Dispose();
                }
                catch (OperationCanceledException) { }
                finally
                {
                    cts?.Dispose();
                    cts = null;
                }
            });
        }
    }
}