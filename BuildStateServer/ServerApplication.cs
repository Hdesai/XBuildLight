using System;
using System.ServiceModel;
using BuildClient;

namespace BuildStateServer
{
    internal class ServerApplication
    {
        static ServiceHost _serviceHost;

        public static void Start()
        {
            
            try
            {
                Tracing.Server.TraceInformation("Starting Build Status Monitor");
                _serviceHost = new ServiceHost(typeof(BuildStatusChangeService));
                _serviceHost.Open();
                Tracing.Server.TraceInformation("Build Status Monitor is running");
            }
            catch (Exception exception)
            {
                Tracing.Server.TraceInformation("Build Status Monitor encountered an exception" + exception);
            }
            
        }

        public static void Stop()
        {

            Tracing.Server.TraceInformation("Stopping Build Status Monitor");

            try
            {
                if (_serviceHost != null) _serviceHost.Close();
            }
            catch (Exception)
            {
                if (_serviceHost != null) _serviceHost.Abort();
            }

            Tracing.Server.TraceInformation("Build Status Monitor Stopped");
        }
    }
}