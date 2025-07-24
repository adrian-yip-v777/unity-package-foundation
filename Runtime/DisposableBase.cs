using System;

namespace vz777.Foundation
{
    /// <summary>
    /// A common, basic disposing handling for an object.
    /// </summary>
    public class DisposableBase : IDisposable
    {
        ~DisposableBase()
        {
            Dispose(false);
        }
        
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            
            if (disposing)
                DisposeManagedResources();
            
            DisposeUnManagedResources();
            IsDisposed = true;
        }

        /// <summary>
        /// Release unmanaged resources here (if any)
        /// e.g., native handles, memory allocated via Marshal, etc.
        /// </summary>
        protected virtual void DisposeUnManagedResources() { }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        protected virtual void DisposeManagedResources() { }
    }
}

