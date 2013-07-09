using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildCommon
{
    public static class TaskExtensions
    {
        public static void WithContinuation(this Task task,
            Action successAction,
            Action exceptionAction, Action cancellationAction)
        {
            task.ExecuteContinueWithInternal(successAction, exceptionAction, cancellationAction);
        }

        public static void ExecuteContinueWithInternal(this Task task,Action successAction,Action exceptionAction,
            Action cancellationAction)
        {
            task.ContinueWith(p => successAction(),
                TaskContinuationOptions.OnlyOnRanToCompletion);

            task.ContinueWith(p => exceptionAction(), TaskContinuationOptions.NotOnFaulted);

            task.ContinueWith(p => cancellationAction(),
                              TaskContinuationOptions.OnlyOnCanceled);
        }
    }
}
