using System.ServiceModel;

namespace BuildCommon
{
    [ServiceContract]
    public interface IBuildStatusChange
    {
        [OperationContract(IsOneWay = true)]
        void OnBuildFailed();

        [OperationContract(IsOneWay = true)]
        void OnBuildStarted();

        [OperationContract(IsOneWay = true)]
        void OnBuildStopped();

        [OperationContract(IsOneWay = true)]
        void OnBuildPartiallySucceeded();

        [OperationContract(IsOneWay = true)]
        void OnBuildInProgress();

        [OperationContract(IsOneWay = true)]
        void OnBuildNotStarted();

        [OperationContract(IsOneWay = true)]
        void OnBuildSuceeded();

        [OperationContract(IsOneWay = true)]
        void OnBuildQualityChange(string quality);
    }
}