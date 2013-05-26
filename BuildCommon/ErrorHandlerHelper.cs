using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BuildCommon
{
    internal static class ErrorHandlerHelper
    {

        

        public static void PromoteException(Type serviceType, Exception error, MessageVersion version, ref Message fault)
        {
            //Is error in the form of FaultException<T> ? 
            if (error.GetType().IsGenericType && error is FaultException)
            {
                Debug.Assert(error.GetType().GetGenericTypeDefinition() == typeof (FaultException<>));
                return;
            }


            bool inContract = ExceptionInContract(serviceType, error);
            if (inContract == false)
            {
                return;
            }
            try
            {
                Type faultUnboundedType = typeof (FaultException<>);
                Type faultBoundedType = faultUnboundedType.MakeGenericType(error.GetType());

                Type[] parameter1 = {typeof (string)};
                ConstructorInfo info = error.GetType().GetConstructor(parameter1);
                Debug.Assert(info != null, "Exception type " + error.GetType() + " does not have suitable constructor");

                var newException = (Exception) Activator.CreateInstance(error.GetType(), error.Message);


                Type[] parameter2 = {newException.GetType()};
                info = faultBoundedType.GetConstructor(parameter2);
                Debug.Assert(info != null, "Exception type " + faultBoundedType + " does not have suitable constructor");

                var faultException = (FaultException) Activator.CreateInstance(faultBoundedType, newException);

                MessageFault messageFault = faultException.CreateMessageFault();
                fault = Message.CreateMessage(version, messageFault, faultException.Action);
            }
            catch
            {
            }
        }

        //Can only be called inside a service
        public static void PromoteException(Exception error, MessageVersion version, ref Message fault)
        {
            var frame = new StackFrame(1);

            Type serviceType = frame.GetMethod().ReflectedType;
            PromoteException(serviceType, error, version, ref fault);
        }

        public static void LogError(Exception error)
        {
            LogError(error, null);
        }

        private static bool ExceptionInContract(Type serviceType, Exception error)
        {
            var faultAttributes = new List<FaultContractAttribute>();

            if (serviceType == null)
            {
                return false;
            }

            Type[] interfaces = serviceType.GetInterfaces();

            if (interfaces.Length <= 0)
            {
                return false;
            }

            string serviceMethod = GetServiceMethod(error);

            foreach (Type interfaceType in interfaces)
            {
                MethodInfo[] methods = interfaceType.GetMethods();


                foreach (MethodInfo methodInfo in methods)
                {
                    if (methodInfo.Name == serviceMethod) //Does not deal with overlaoded methods 
                        //or same method name on different contracts implemented explicitly 
                    {
                        IEnumerable<FaultContractAttribute> attributes = GetFaults(methodInfo);
                        faultAttributes.AddRange(attributes);
                        return FindError(faultAttributes, error);
                    }
                }
            }
            return false;
        }

        private static string GetServiceMethod(Exception error)
        {
            const string wcfPrefix = "SyncInvoke";
            if (error.StackTrace != null)
            {
                int start = error.StackTrace.IndexOf(wcfPrefix, StringComparison.Ordinal);
                //Debug.Assert(start != -1);//Did they change the prefix???

                string trimedTillMethod = error.StackTrace.Substring(start + wcfPrefix.Length);
                string[] parts = trimedTillMethod.Split('(');
                return parts[0];
            }
            return null;
        }

        private static IEnumerable<FaultContractAttribute> GetFaults(MethodInfo methodInfo)
        {
            object[] attributes = methodInfo.GetCustomAttributes(typeof (FaultContractAttribute), false);
            return attributes as FaultContractAttribute[];
        }

        private static bool FindError(List<FaultContractAttribute> faultAttributes, Exception error)
        {
            Predicate<FaultContractAttribute> sameFault = (fault) =>
                {
                    Type detailType = fault.DetailType;
                    return detailType == error.GetType();
                };
            return faultAttributes.Exists(sameFault);
        }

        public static void LogError(Exception error, MessageFault fault)
        {
            LogData entry = CreateLogbookentry(error, fault);
            Tracing.ErrorHandler.TraceError(entry.ToString());
        }

        private static LogData CreateLogbookentry(Exception error, MessageFault fault)
        {
            string typeName, methodName;

            string assemblyName = typeName = methodName = "Unknown";

            if (error.TargetSite != null)
            {
                assemblyName = error.TargetSite.Module.Assembly.GetName().Name;
                methodName = error.TargetSite.Name;
                if (error.TargetSite.DeclaringType != null) typeName = error.TargetSite.DeclaringType.Name;
            }

            string fileName = GetFileName(error);
            int lineNumber = GetLineNumber(error);
            string exceptionName = error.GetType().ToString();
            string exceptionMessage = error.Message;
            string providedFault = String.Empty;
            string providedMessage = String.Empty;

            if (fault != null)
            {
                providedFault = (fault.Code == null) ? "Unknown" : fault.Code.Name;

                providedMessage = (fault.Reason == null || fault.Reason.Translations == null ||
                                   fault.Reason.Translations.Count <= 0)
                                      ? "Unknown"
                                      : fault.Reason.Translations[0].Text;
            }
            return new LogData(assemblyName, fileName, lineNumber, typeName, methodName, exceptionName, exceptionMessage,
                               providedFault, providedMessage);
        }

        private static string GetFileName(Exception error)
        {
            string returnFileName = String.Empty;
            try
            {
                if (error == null || error.StackTrace == null)
                {
                    return "Unavailable";
                }
                int originalLineIndex = error.StackTrace.IndexOf(":line", StringComparison.Ordinal);
                if (originalLineIndex == -1)
                {
                    return "Unavailable";
                }
                string originalLine = error.StackTrace.Substring(0, originalLineIndex);

                if (String.IsNullOrEmpty(originalLine))
                {
                    return "Unavailable";
                }

                string[] sections = originalLine.Split('\\');
                if (sections.Length <= 0)
                {
                    return "Unavailable";
                }

                returnFileName = sections[sections.Length - 1];
            }
            catch
            {
            }

            return returnFileName;
        }

        private static int GetLineNumber(Exception error)
        {
            if (error == null || error.StackTrace == null)
            {
                return 0;
            }
            string[] sections = error.StackTrace.Split(' ');

            int index = sections.TakeWhile(section => !section.EndsWith(":line")).Count();
            Debug.Assert(index != 0);
            if (index == sections.Length)
            {
                return 0;
            }
            string lineNumber = sections[index + 1];
            int number = -1;
            try //Strip the /r/n if present
            {
                number = Convert.ToInt32(lineNumber.Substring(0, lineNumber.Length - 2));
            }
            catch (FormatException)
            {
                number = Convert.ToInt32(lineNumber);
            }

            return number;
        }
    }

    public static class BuildClientExceptionHelper
    {
      
    }
}