using System;
using System.ServiceModel;

namespace BuildCommon
{
    public static class BuildStatusChangeExtension
    {
       
        public static void EnsureDisposed<T>(this T channel)
        {
            var clientChannel = channel as IClientChannel;
            if (clientChannel != null)
            {
                try
                {
                    clientChannel.Dispose();
                }
                catch (Exception)
                {
                    clientChannel.Abort();
                }
            }
        }

        public static T ExecuteOneWayCall<T>(this T channel, 
                                                     Action<T> action)
        {
            try
            {
                channel.EnsureOpened();
                action(channel);
            }
            finally
            {
                channel.EnsureDisposed();
            }
            return default(T);
        }

        public static T EnsureOpened<T>(this T channel) 
        {
            var clientChannel = channel as IClientChannel;
            if (clientChannel != null)
            {
                try
                {
                    clientChannel.Open();
                }
                catch (CommunicationException)
                {
                    Console.WriteLine(
                        "Could not open Proxy as channel is in faulted state, State Service may not be running!!");
                    throw;
                }

                return channel;
            }

            return default(T);
        }
    }
}