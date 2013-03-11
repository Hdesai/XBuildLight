using System;
using System.Threading;
using Autofac;
using BuildClient.Configuration;
using BuildCommon;

namespace BuildClient
{
    internal class ClientApplication
    {
        //private static Timer _stateTimer;
        private static BuildManager _buildManager;


        public static void Start()
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) =>
                Tracing.Server.TraceError(
                    "Server-ApplicationException-An unhandled application exception occured-details {0}",
                    e.ExceptionObject.ToString());

            var container = LoadContainer();

            _buildManager = new BuildManager(
                
                container.Resolve<IBuildConfigurationManager>(),
                container.Resolve<INotifier>(),
                container.Resolve<IBuildStoreEventSource>()
                
                );
            

            Tracing.Server.TraceInformation("Starting Server");
            ThreadPool.QueueUserWorkItem(x => StartTimer());
        }

        private static IContainer LoadContainer()
        {
            var builder=new ContainerBuilder();
            builder.RegisterType<BuildConfigurationManager>().As<IBuildConfigurationManager>();
            builder.RegisterType<Notifier>().As<INotifier>();
            builder.RegisterType<BuildStoreEventSource>().As<IBuildStoreEventSource>();
            builder.Register(c=>new TfsServiceProvider(c.Resolve<IBuildConfigurationManager>().TeamFoundationUrl)).As<IServiceProvider>();
            builder.RegisterType<BuildStatusChangeProxy>().As<IBuildStatusChange>();

            return builder.Build();


        }


        public static void Stop()
        {
            ThreadPool.QueueUserWorkItem(x => StopTimer());
        }

        private static void StartTimer()
        {
            Tracing.Server.TraceInformation("Starting Timer");
            _buildManager.StartProcessing(null);
        }

        private static void StopTimer()
        {
            //_stateTimer.Dispose();
            Tracing.Server.TraceInformation("\nDestroying timer.");
            Tracing.Server.TraceInformation("Stopping Timer");

            _buildManager.StopProcessing();
            Tracing.Server.TraceInformation("Timer Stopped");
        }
    }
}