using System;
using System.Collections.Generic;
using System.Text;

namespace plenidev.AnsiblePlayer.Utils
{
    internal class MaybeDisposer<T>(T resource, bool disposing = true) : IDisposable where T : IDisposable
    {
        public T Resource => resource;
        public void Dispose()
        {
            if (disposing) resource.Dispose();
        }
    }
}
