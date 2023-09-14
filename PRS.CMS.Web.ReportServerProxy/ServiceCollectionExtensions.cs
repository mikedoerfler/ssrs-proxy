using System.Net;
using Yarp.ReverseProxy.Configuration;

namespace PRS.CMS.Web.ReportServerProxy;

public static class ServiceCollectionExtensions
{
    public static void AddYarp(this IServiceCollection services)
    {
        // these RouteConfig would be hard coded - no need to make any of them different
        var reportServerRouteConfig = new RouteConfig
        {
            RouteId = "ReportServer",
            ClusterId = "cluster1",
            Match = new RouteMatch
            {
                Path = "ReportServer/{**catch-all}"
            }
        };

        var reportsRouteConfig = new RouteConfig
        {
            RouteId = "Reports",
            ClusterId = "cluster1",
            Match = new RouteMatch
            {
                Path = "Reports/{**catch-all}"
            },
            AuthorizationPolicy = "Default"
        };

        // the DestinationConfig would need a config that points to the internal report server url
        var clusterConfigs = new ClusterConfig
        {
            ClusterId = "cluster1",
            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "internal", new DestinationConfig
                    {
                        Address = "http://localhost"
                    }
                }
            }
        };

        services.AddReverseProxy()
            .ConfigureHttpClient((_, handler) => { handler.Credentials = CredentialCache.DefaultCredentials; })
            .LoadFromMemory(new[] { reportServerRouteConfig, reportsRouteConfig }, new[] { clusterConfigs })
            .AddTransforms(builderContext => { builderContext.ResponseTransforms.Add(new CustomResponseTransform()); });
    }
}