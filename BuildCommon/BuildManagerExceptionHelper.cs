using System;
using System.Net;
using System.ServiceModel;


namespace BuildCommon
{
    public static class BuildManagerExceptionHelper
    {
        public static void With(string serviceAddress, Action taction, Action preAction, Action postAction)
        {
            if (preAction != null)
            {
                preAction();
            }

            try
            {
                if (taction == null)
                {
                    throw new ArgumentException("taction can not be null");
                }
                taction();
            }
            catch (EndpointNotFoundException)
            {
                Tracing.Client.TraceError(
                    String.Format("No service is listening at address [{0}] to accept build light commands",
                                  serviceAddress));
            }

            catch (CommunicationException exception)
            {
                Tracing.Client.TraceError(
                    String.Format(
                        "There was a problem with communication while communicating with notifier at address {0} ",
                        serviceAddress));
                Tracing.Client.TraceError(exception.ToString());
            }
            catch (TimeoutException exception)
            {
                Tracing.Client.TraceError(
                    String.Format(
                        "Service Operation timed out while trying to communicate with notifier at address {0} ",
                        serviceAddress));
                Tracing.Client.TraceError(exception.ToString());
            }
            catch (WebException webEx)
            {
                Tracing.Client.TraceError(
                    String.Format(
                        "There was a problem with communication while communicating with notifier at address {0} ",
                        serviceAddress));
                Tracing.Client.TraceError(webEx.ToString());
            }

            finally
            {
                if (postAction != null)
                {
                    postAction();
                }

            }


        }

        public static void WithExceptionHandling(Action taction, Action preAction, Action postAction)
        {
            
            preAction();

            try
            {
                taction();

            }
            //catch (Microsoft.TeamFoundation.Framework.Client.DatabaseConnectionException exception)
            //{
            //    Tracing.Client.TraceError(String.Format("An Exception Occured while connecting TfsServer {0}", exception));
            //}
            //catch (Microsoft.TeamFoundation.Build.Client.BuildServerException exception)
            //{
            //    Tracing.Client.TraceError(String.Format("An Exception Occured while connecting TfsServer {0}", exception));
            //}
            //catch (Microsoft.TeamFoundation.TeamFoundationServiceUnavailableException exception)
            //{
            //    Tracing.Client.TraceError(String.Format("An Exception Occured while connecting TfsServer {0}", exception));
            //}

            catch (WebException exception)
            {
                Tracing.Client.TraceError(String.Format("An Exception Occured while connecting TfsServer {0}", exception));
            }
            catch (AggregateException exception)
            {
                var message = exception.UnWrapAggregateExcetion();
                Tracing.Client.TraceError(String.Format("An Exception Occured while connecting TfsServer {0} ", message));
            }
            catch (Exception exception)
            {
                Tracing.Client.TraceError(String.Format(
                    "An Unhandled Exception Occured while connecting TfsServer {0} ", exception));
                throw;
            }

            finally
            {
                postAction();
            }
        }
    }
}