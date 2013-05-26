using System;
using System.Reflection;
using System.Text;

namespace BuildCommon
{
    public static class ExceptionHelpers
    {
        public static void ThrowIfReflectionTypeLoadThrowResolutionException(this Exception ex)
        {
            if (ex is ReflectionTypeLoadException)
            {
                var typeLoadException = ex as ReflectionTypeLoadException;
                Exception[] loaderExceptions = typeLoadException.LoaderExceptions;
                throw new AggregateException(typeLoadException.Message, loaderExceptions);
            }
        }

        public static string UnWrapAggregateExcetion(this AggregateException aggregateException)
        {
            var stringBuilder = new StringBuilder();

            var innerExceptions = aggregateException.Flatten().InnerExceptions;
            foreach (var vr in innerExceptions)
            {
                stringBuilder.AppendLine(vr.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}