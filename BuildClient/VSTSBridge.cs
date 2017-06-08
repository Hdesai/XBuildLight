using System;
using System.Collections.Generic;
using BuildClient.Configuration;
using Microsoft.VisualStudio.Services.Common;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;

namespace BuildClient
{
    public class VSTSBridge : IBuildEventSystem
    {
        private readonly IBuildConfigurationManager _config;
        public VSTSBridge(IBuildConfigurationManager config,string url, string personalAccessToken)
        {
            _config = config;
            _credentials = new VssBasicCredential("", personalAccessToken);
            _uri = new Uri(url);
        }

        public async Task<IEnumerable<BuildStoreEventArgs>> GetBuildStoreEvents()
        {
            var projects = await GetListOfProjects();
            var buildIds = new List<int>();
            

            var builds = new List<BuildStoreEventArgs>();
            foreach (var project in projects)
            {
                var allBuilds = await GetBuilds(project.Id,buildIds.ToArray());
                foreach (var build in allBuilds)
                {
                    var buildStoreArg = new BuildStoreEventArgs();
                    //data
                    buildStoreArg.Data = new BuildData();
                    buildStoreArg.Data.BuildName = build.Value.Definition.Name;
                    buildStoreArg.Data.BuildRequestedFor = build.Value.RequestedFor.DisplayName;
                    buildStoreArg.Data.Quality = build.Value.Quality;
                    buildStoreArg.Data.Status = GetStatus(build.Value.Status,build.Value.Result);
                    //
                    buildStoreArg.Type = BuildStoreEventType.Build;
                }
            }

            return null;

        }

        public BuildExecutionStatus GetStatus(Microsoft.TeamFoundation.Build.WebApi.BuildStatus? status,BuildResult? result)
        {
            switch (status)
            {
                case Microsoft.TeamFoundation.Build.WebApi.BuildStatus.All:
                    break;
                case Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress:
                    return BuildExecutionStatus.InProgress;
                case Microsoft.TeamFoundation.Build.WebApi.BuildStatus.None:
                    break;
                case Microsoft.TeamFoundation.Build.WebApi.BuildStatus.NotStarted:
                    break;
                case Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed:
                    if (!result.HasValue)
                    {
                        break;
                    }

                    if (result==BuildResult.Failed)
                    {
                        return BuildExecutionStatus.Failed;
                    }
                    else if (result==BuildResult.Succeeded)
                    {
                        return BuildExecutionStatus.Succeeded;

                    }
                    else if (result==BuildResult.PartiallySucceeded)
                    {
                        return BuildExecutionStatus.PartiallySucceeded;
                    }
                    break;
            }

            return BuildExecutionStatus.Unknown;
        }

        private VssBasicCredential _credentials;
            private Uri _uri;

       
        public async Task<IEnumerable<TeamProjectReference>> GetListOfProjects()
        {
            using (ProjectHttpClient projectHttpClient = new ProjectHttpClient(_uri, _credentials))
            {
                return await projectHttpClient.GetProjects();
            }
        }

        public async Task<Dictionary<int, Build>> GetBuilds(Guid projectId, int[] ids)
        {
            var dict = new Dictionary<int, Build>();
            foreach (var id in ids)
            {
                if (!dict.ContainsKey(id))
                {
                    dict.Add(id, null);
                }
            }

            using (BuildHttpClient buildHttpClient = new BuildHttpClient(_uri, _credentials))
            {
                var buildDefs = await buildHttpClient.GetDefinitionsAsync(projectId, definitionIds: ids);
                var allBuilds = await buildHttpClient.GetBuildsAsync2(projectId, maxBuildsPerDefinition: 1, queryOrder: Microsoft.TeamFoundation.Build.WebApi.BuildQueryOrder.FinishTimeDescending, statusFilter: Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress | Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed);
                foreach (var def in buildDefs)
                {
                    foreach (var build in allBuilds)
                    {
                        if (build.Definition.Id == def.Id)
                        {
                            dict[def.Id] = build;
                            break;
                        }
                    }
                }
            }
            return dict;
        }

    }
}