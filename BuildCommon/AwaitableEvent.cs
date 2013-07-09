using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildCommon
{
    public sealed class AwaitableEvent<TEventArgs>
    {
        private TaskCompletionSource<AwaitableEventArgs<TEventArgs>> _tcs;

        public Task<AwaitableEventArgs<TEventArgs>> RaiseAsync()
        {
            if (_tcs==null)
            {
                _tcs=new TaskCompletionSource<AwaitableEventArgs<TEventArgs>>();
            }

            return _tcs.Task;
        }

        public void EventHandler(Object sender,TEventArgs eventArgs)
        {
            if (_tcs==null)
            {
                return;
            }

            var tcs = _tcs;
            _tcs = null;

            tcs.SetResult(new AwaitableEventArgs<TEventArgs>(sender,eventArgs));
        }
    }

    public sealed class AwaitableEventArgs<TEventArgs>
    {
        private readonly Object Sender;
        private readonly TEventArgs Args;

        internal AwaitableEventArgs(Object sender,TEventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }
}
