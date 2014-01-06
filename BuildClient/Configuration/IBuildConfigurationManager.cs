namespace BuildClient.Configuration
{
    public interface IBuildConfigurationManager
    {
        string PollPeriod { get; }
        string BuildDefinitionNameExclusionPattern { get; }
        string TeamFoundationUrl { get; }
        BuildMapperGroupElementCollection BuildMappers { get; }
        bool UseCredentialToAuthenticate { get; }
        string TfsAccountDomain { get; }
        string TfsAccountUserName { get; }
        string TfsAccountPassword { get; }

    }
}