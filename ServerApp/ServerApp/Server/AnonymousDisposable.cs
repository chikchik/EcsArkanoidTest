using System;
using System.Threading.Tasks;

namespace ServerApp.Server
{
    public class AnonymousDisposable : IAsyncDisposable
    {
        private readonly Func<Task> _callback;

        public AnonymousDisposable(Func<Task> callback)
        {
            _callback = callback;
        }

        public async ValueTask DisposeAsync()
        {
            await _callback();
        }
    }
}