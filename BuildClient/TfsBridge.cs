using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using BuildClient.Configuration;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Server;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildClient
{
    public class TfsBridge : IBuildEventSystem
    {

        private readonly IDictionary<string, IBuildDetail> _cacheLookup = new Dictionary<string, IBuildDetail>();
        private readonly IBuildConfigurationManager _buildConfigurationManager;
        private readonly Regex _buildDefinitionNameExclusionRegex;
        private readonly IBuildServer _buildServer;

        private readonly IServiceProvider _teamFoundationServiceProvider;
        public TfsBridge(IServiceProvider teamFoundationServiceProvider,
                                     IBuildConfigurationManager buildConfigurationManager)
            : this(teamFoundationServiceProvider, String.Empty, buildConfigurationManager)
        {
        }

        public TfsBridge(IServiceProvider teamFoundationServiceProvider,
                                     string buildDefinitionNameExclusionPattern,
                                     IBuildConfigurationManager buildConfigurationManager)
        {
            _teamFoundationServiceProvider = teamFoundationServiceProvider;
            _buildConfigurationManager = buildConfigurationManager;

            if (!string.IsNullOrEmpty(buildDefinitionNameExclusionPattern))
            {
                _buildDefinitionNameExclusionRegex = new Regex(buildDefinitionNameExclusionPattern,
                                                               RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            _buildServer = _teamFoundationServiceProvider.GetService<IBuildServer>();
        }


        //Will be polled periodically
        public async Task<IEnumerable<BuildStoreEventArgs>> GetBuildStoreEvents()
        {
            IEnumerable<string> teamProjectNames = GetTeamProjectNames();
            var  builds = GetBuildsForTeamProjects(teamProjectNames);
            var allEvents = new List<BuildStoreEventArgs>();

            foreach (IBuildDetail build in builds)
            {
                BuildStoreEventArgs buildStoreEvent = GetBuildStoreEventIfAny(build);
                if (buildStoreEvent != null)
                {
                    allEvents.Add(buildStoreEvent);
                }
            }

            return await Task.FromResult<IEnumerable<BuildStoreEventArgs>>(allEvents.ToArray());
        }

        private IEnumerable<string> GetTeamProjectNames()
        {
            BuildMapperElement[] projectsToMonitor = _buildConfigurationManager
                .BuildMappers
                .OfType<BuildMapperElement>().ToArray();
            if (!projectsToMonitor.Any())
            {
                return Enumerable.Empty<string>();
            }

            var projectNames = new List<string>();

            foreach (BuildMapperElement mapperElement in projectsToMonitor)
            {
                if (String.IsNullOrEmpty(mapperElement.TfsProjectToMonitor))
                {
                    throw new ConfigurationErrorsException(
                        "TFS Project to monitor is not specified, please provide the TFS project.");
                }

                var structureService = _teamFoundationServiceProvider.GetService<ICommonStructureService>();
                var projectInfos = structureService.ListProjects();
                BuildMapperElement element = mapperElement;
                IEnumerable<string> projectInfoNames =
                    projectInfos.Select(p => p.Name)
                        .Where(x => x == element.TfsProjectToMonitor);
                projectNames.AddRange(projectInfoNames);
            }

            return projectNames.Distinct();
        }


        private IEnumerable<IBuildDetail> GetBuildsForTeamProjects(IEnumerable<string> teamProjectNames)
        {

            var builds_ = _buildServer.QueryBuildDefinitions("zenith", QueryOptions.Definitions);

            foreach (string teamProjectName in teamProjectNames)
            {

                IBuildDefinition[] definitions = _buildServer.QueryBuildDefinitions(teamProjectName);
                foreach (IBuildDefinition definition in definitions)
                {
                    if (ShouldDefinitionBeIncluded(definition))
                    {

                        var buildSpec = _buildServer.CreateBuildDetailSpec(teamProjectName, definition.Name);
                        buildSpec.InformationTypes = null;
                        buildSpec.MaxBuildsPerDefinition = 1;
                        buildSpec.QueryOrder = Microsoft.TeamFoundation.Build.Client.BuildQueryOrder.FinishTimeDescending;
                        IBuildQueryResult result = _buildServer.QueryBuilds(buildSpec);
                        yield return result.Builds.FirstOrDefault();
                    }
                }
            }
        }

        private bool ShouldDefinitionBeIncluded(IBuildDefinition definition)
        {
            if (_buildDefinitionNameExclusionRegex != null)
            {
                bool isExcludedByRegex = _buildDefinitionNameExclusionRegex.IsMatch(definition.Name);

                return (definition.QueueStatus == Microsoft.TeamFoundation.Build.Client.DefinitionQueueStatus.Enabled)
                       && !isExcludedByRegex;
            }
            IEnumerable<BuildMapperElement> buildMapperElements =
                _buildConfigurationManager.BuildMappers.OfType<BuildMapperElement>();

            return (definition.QueueStatus == Microsoft.TeamFoundation.Build.Client.DefinitionQueueStatus.Enabled) &&
                   buildMapperElements.Any(x => x.TfsBuildToMonitor == definition.Name);
        }

        private BuildStoreEventArgs GetBuildStoreEventIfAny(IBuildDetail build)
        {

            BuildStoreEventArgs buildStoreEvent;
            if (!_cacheLookup.ContainsKey(build.Uri.AbsoluteUri))
            {
                _cacheLookup.Add(build.Uri.AbsoluteUri, build);

                buildStoreEvent = new BuildStoreEventArgs
                {
                    Type = BuildStoreEventType.Build,
                    Data = Generate(build)
                };
                return buildStoreEvent;
            }

            IBuildDetail originalBuild = _cacheLookup[build.Uri.AbsoluteUri];
            _cacheLookup[build.Uri.AbsoluteUri] = build;

            //Handle quality change event
            if (originalBuild.Quality != build.Quality || originalBuild.Status != build.Status)
            {
                buildStoreEvent = new BuildStoreEventArgs
                {
                    Data = Generate(build),
                    Type = originalBuild.Quality != build.Quality
                        ? BuildStoreEventType.QualityChanged
                        : BuildStoreEventType.Build
                };
                return buildStoreEvent;
            }

            return new BuildStoreEventArgs
            {
                Data = Generate(build),
                Type = BuildStoreEventType.Build
            };
        }



        public BuildData Generate(IBuildDetail buildDetail)
        {
            return new BuildData
            {
                BuildName = buildDetail.BuildDefinition.Name,
                BuildRequestedFor = buildDetail.RequestedFor,
                Status = GetStatus(buildDetail.Status),
                Quality = buildDetail.Quality
            };
        }

        public BuildExecutionStatus GetStatus(Microsoft.TeamFoundation.Build.Client.BuildStatus status)
        {

            switch (status)
            {
                case Microsoft.TeamFoundation.Build.Client.BuildStatus.All:

                case Microsoft.TeamFoundation.Build.Client.BuildStatus.Failed:
                    return BuildExecutionStatus.Failed;

                case Microsoft.TeamFoundation.Build.Client.BuildStatus.InProgress:
                    return BuildExecutionStatus.InProgress;

                case Microsoft.TeamFoundation.Build.Client.BuildStatus.None:

                case Microsoft.TeamFoundation.Build.Client.BuildStatus.NotStarted:

                case Microsoft.TeamFoundation.Build.Client.BuildStatus.PartiallySucceeded:
                    return BuildExecutionStatus.PartiallySucceeded;

                case Microsoft.TeamFoundation.Build.Client.BuildStatus.Stopped:
                    return BuildExecutionStatus.Stopped;

                case Microsoft.TeamFoundation.Build.Client.BuildStatus.Succeeded:
                    return BuildExecutionStatus.Succeeded;
            }

            return BuildExecutionStatus.Unknown;
        }
    }
}