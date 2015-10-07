using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using BuildClient.Configuration;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Server;

namespace BuildClient
{
    public interface IBuildEventSystem
    {
        IEnumerable<BuildStoreEventArgs> GetBuildStoreEvents();
    }

    public class TfsBridge:IBuildEventSystem
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
        public IEnumerable<BuildStoreEventArgs> GetBuildStoreEvents()
        {
            IEnumerable<string> teamProjectNames = GetTeamProjectNames();
            IEnumerable<IBuildDetail> builds = GetBuildsForTeamProjects(teamProjectNames);

            foreach (IBuildDetail build in builds)
            {
                BuildStoreEventArgs buildStoreEvent = GetBuildStoreEventIfAny(build);
                if (buildStoreEvent != null)
                {
                    yield return buildStoreEvent;
                }
            }
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
                ProjectInfo[] projectInfos = structureService.ListProjects();
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
                        buildSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
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

                return (definition.QueueStatus == DefinitionQueueStatus.Enabled)
                       && !isExcludedByRegex;
            }
            IEnumerable<BuildMapperElement> buildMapperElements =
                _buildConfigurationManager.BuildMappers.OfType<BuildMapperElement>();

            return (definition.QueueStatus == DefinitionQueueStatus.Enabled) &&
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
                EventType = BuildStoreEventType.Build,
                Status = GetStatus(buildDetail.Status),
                Quality = buildDetail.Quality
            };
        }

        public BuildExecutionStatus GetStatus(BuildStatus status)
        {
           
            switch (status)
            {
                case BuildStatus.All:
                    
                case BuildStatus.Failed:
                    return BuildExecutionStatus.Failed;
                    
                case BuildStatus.InProgress:
                    return BuildExecutionStatus.InProgress;

                case BuildStatus.None:
                    
                case BuildStatus.NotStarted:
                    
                case BuildStatus.PartiallySucceeded:
                    return BuildExecutionStatus.PartiallySucceeded;

                case BuildStatus.Stopped:
                    return BuildExecutionStatus.Stopped;

                case BuildStatus.Succeeded:
                    return BuildExecutionStatus.Succeeded;
            }

            return BuildExecutionStatus.Unknown;
        }
    }
}