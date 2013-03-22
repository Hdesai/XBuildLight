using System;
using System.Reflection;

namespace BuildCommon
{
    public class ExceptionHelpers
    {
        public static void ThrowIfReflectionTypeLoadThrowResolutionException(Exception ex)
        {
            if (ex is ReflectionTypeLoadException)
            {
                var typeLoadException = ex as ReflectionTypeLoadException;
                Exception[] loaderExceptions = typeLoadException.LoaderExceptions;
                throw new AggregateException(typeLoadException.Message, loaderExceptions);
            }
        }
    }
}