using System;
using System.Reflection;
using System.Threading;
using Autofac;
using Autofac.Core;
using BuildClient.Configuration;
using BuildCommon;

namespace BuildClient
{
    internal class ClientApplication
    {
        
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

            Tracing.Server.TraceInformation("Starting Build Manager");
            ThreadPool.QueueUserWorkItem(x => StartBuildManager());
        }

        private static IContainer LoadContainer()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.RegisterType<BuildConfigurationManager>().As<IBuildConfigurationManager>();
                builder.RegisterType<Notifier>().As<INotifier>();
                builder.RegisterType<BuildStoreEventSource>().As<IBuildStoreEventSource>();
                builder.Register(c => new TfsServiceProvider(c.Resolve<IBuildConfigurationManager>().TeamFoundationUrl)).As<IServiceProvider>();
                builder.RegisterType<BuildStatusChangeProxy>().As<IBuildStatusChange>();

                return builder.Build();
            }
            catch (Exception ex)
            {
                ExceptionHelpers.ThrowIfReflectionTypeLoadThrowResolutionException(ex);
                throw;
            }
        }

        

        public static void Stop()
        {
            ThreadPool.QueueUserWorkItem(x => StopBuildManager());
        }

        private static void StartBuildManager()
        {
            
            _buildManager.StartProcessing(null);
        }

        private static void StopBuildManager()
        {
            
            if (_buildManager!=null)
            {
                _buildManager.StopProcessing();
                Tracing.Server.TraceInformation("Build Manager Stopped");
            }
          
        }
    }
}