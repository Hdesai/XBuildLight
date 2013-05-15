using System;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using Autofac;
using Autofac.Core;
using BuildClient.Configuration;
using BuildCommon;

namespace BuildClient
{
    internal class ClientApplication : IDisposable
    {
        private BuildManager _buildManager;

        public void Start()
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
            ThreadPool.QueueUserWorkItem(x => _buildManager.StartProcessing(null));
        }

        private static IContainer LoadContainer()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.RegisterType<BuildConfigurationManager>().As<IBuildConfigurationManager>();
                builder.RegisterType<Notifier>().As<INotifier>();
              
                builder.RegisterType<BuildStoreEventSource>().As<IBuildStoreEventSource>().SingleInstance();
                builder.Register(c => new TfsServiceProvider(c.Resolve<IBuildConfigurationManager>().TeamFoundationUrl))
                       .As<IServiceProvider>();
                builder.RegisterType<BuildStatusChangeProxy>().As<IBuildStatusChange>().OnRelease(ReleaseProxy);

                return builder.Build();
            }
            catch (Exception ex)
            {
                ExceptionHelpers.ThrowIfReflectionTypeLoadThrowResolutionException(ex);
                throw;
            }
        }

        private static void ReleaseProxy(BuildStatusChangeProxy proxy)
        {
            if (proxy == null) return;
            try
            {
                if (proxy.State == CommunicationState.Opened || proxy.State == CommunicationState.Created)
                {
                    proxy.Close();
                }
            }
            catch (Exception)
            {
                proxy.Abort();
            }
        }

        public void Stop()
        {
            ThreadPool.QueueUserWorkItem(x =>
                {
                    if (_buildManager != null)
                    {
                        _buildManager.StopProcessing();
                        Tracing.Server.TraceInformation("Build Manager Stopped");
                    }
                });
        }


        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_buildManager != null)
                    {
                        _buildManager.Dispose();
                    }
                }

                _buildManager = null;

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}