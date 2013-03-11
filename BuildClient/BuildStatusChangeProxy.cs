using System.ServiceModel;
using BuildCommon;

namespace BuildClient
{
    public class BuildStatusChangeProxy:ClientBase<IBuildStatusChange>,IBuildStatusChange
    {
        public void OnBuildFailed()
        {
            Channel.OnBuildFailed();
        }

        public void OnBuildStarted()
        {
            Channel.OnBuildStarted();
        }

        public void OnBuildStopped()
        {
            Channel.OnBuildFailed();
        }

        public void OnBuildPartiallySucceeded()
        {
            Channel.OnBuildPartiallySucceeded();
        }

        public void OnBuildInProgress()
        {
            Channel.OnBuildInProgress();
        }

        public void OnBuildNotStarted()
        {
            Channel.OnBuildNotStarted();
        }

        public void OnBuildSuceeded()
        {
            Channel.OnBuildSuceeded();
        }
    }
}