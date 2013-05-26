using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace BuildCommon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ErrorHandlerBehaviorAttribute : Attribute, IErrorHandler, IServiceBehavior
    {
        protected Type ServiceType { get; set; }

        bool IErrorHandler.HandleError(Exception error)
        {
            try
            {
                ErrorHandlerHelper.LogError(error);
            }
            catch (Exception ex)
            {
                try
                {
                    Tracing.ErrorHandler.TraceError(ex.ToString());
                }
                catch
                {
                }
            }

            return false;
        }

        void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            try
            {
                ErrorHandlerHelper.PromoteException(ServiceType, error, version, ref fault);
            }
            catch
            {
            }
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase host)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase host,
                                                   Collection<ServiceEndpoint> endpoints,
                                                   BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
        {
            ServiceType = description.ServiceType;
            foreach (ChannelDispatcher dispatcher in host.ChannelDispatchers)
            {
                dispatcher.ErrorHandlers.Add(this);
            }
        }
    }

    [DataContract]
    public struct LogData
    {
        [DataMember] public readonly string AssemblyName;
        [DataMember] public readonly string Date;
        [DataMember] public readonly string Event;
        [DataMember] public readonly string ExceptionMessage;
        [DataMember] public readonly string ExceptionName;

        [DataMember] public readonly string FileName;
        [DataMember] public readonly string HostName;

        [DataMember] public readonly int LineNumber;
        [DataMember] public readonly string MachineName;

        [DataMember] public readonly string MemberAccessed;

        [DataMember] public readonly string ProvidedFault;

        [DataMember] public readonly string ProvidedMessage;
        [DataMember] public readonly string Time;
        [DataMember] public readonly string TypeName;

        public LogData(string assemblyName, string fileName, int lineNumber, string typeName, string methodName,
                       string exceptionName, string exceptionMessage, string providedFault, string providedMessage,
                       string eventDescription)
            : this(
                assemblyName, fileName, lineNumber, typeName, methodName, exceptionName, exceptionMessage, providedFault,
                providedMessage)
        {
            Event = eventDescription;
        }

        public LogData(string assemblyName, string fileName, int lineNumber, string typeName, string methodName,
                       string exceptionName, string exceptionMessage)
            : this(
                assemblyName, fileName, lineNumber, typeName, methodName, exceptionName, exceptionMessage, String.Empty,
                String.Empty)
        {
        }

        public LogData(string assemblyName, string fileName, int lineNumber, string typeName, string methodName,
                       string exceptionName, string exceptionMessage, string providedFault, string providedMessage)
        {
            MachineName = Environment.MachineName;
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                HostName = Process.GetCurrentProcess().MainModule.ModuleName;
            }
            else
            {
                HostName = entryAssembly.GetName().Name;
            }
            AssemblyName = assemblyName;
            FileName = fileName;
            LineNumber = lineNumber;
            TypeName = typeName;
            MemberAccessed = methodName;
            Date = DateTime.Now.ToShortDateString();
            Time = DateTime.Now.ToLongTimeString();
            ExceptionName = exceptionName;
            ExceptionMessage = exceptionMessage;
            ProvidedFault = providedFault;
            ProvidedMessage = providedMessage;
            Event = String.Empty;
        }
    }
}