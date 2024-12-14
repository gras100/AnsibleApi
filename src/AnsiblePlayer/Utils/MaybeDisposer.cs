using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace plenidev.AnsiblePlayer.Utils
{
    internal struct MaybeDisposer<T>(T resource, bool disposing = true) : IDisposable, IAsyncDisposable where T : IDisposable
    {
        public readonly T Resource => resource;

        public readonly bool Disposing => disposing;

        internal void ForceDispose() => DisposeImpl(disposing = true);

        internal async ValueTask ForceDisposeAsync() => await DisposeAsyncImpl(disposing = true);

        public void Dispose() => DisposeImpl(disposing);

        private void DisposeImpl(bool disposing)
        {
            if (disposing) resource.Dispose();
        }

        public async ValueTask DisposeAsync() => await DisposeAsyncImpl(disposing);
        
        private async ValueTask DisposeAsyncImpl(bool disposing)
        {
            if (disposing)
            {
                if (resource is IAsyncDisposable diposable)
                {
                    await diposable.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    resource.Dispose();
                }
            }
        }
    }
}
