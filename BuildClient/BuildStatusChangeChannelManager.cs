using System;
using System.ServiceModel;
using BuildCommon;

namespace BuildClient
{
    public class BuildStatusChangeChannelManager : IDisposable, ICachedChannelManager<IBuildStatusChange>
    {
        private ChannelFactory<IBuildStatusChange> _channelFactory = new ChannelFactory<IBuildStatusChange>();
        private bool _disposed;

        public IBuildStatusChange CreateChannel(string address)
        {
            var cf =
                new ChannelFactory<IBuildStatusChange>(new NetTcpBinding
                    {
                        Security = new NetTcpSecurity {Mode = SecurityMode.None}
                    });

            return cf.CreateChannel(new EndpointAddress(address));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //Dispose managed resource
                    try
                    {
                        _channelFactory.Close();
                    }
                    catch (Exception)
                    {
                        _channelFactory.Abort();
                    }
                }

                _disposed = true;
            }

            _channelFactory = null;
        }
    }
}