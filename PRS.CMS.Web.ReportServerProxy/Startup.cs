using System.Diagnostics;
using System.Net;

using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

namespace PRS.CMS.Web.ReportServerProxy;

/// <summary>
/// ASP.NET Core pipeline initialization showing how to use IHttpForwarder to directly handle forwarding requests.
/// With this approach you are responsible for destination discovery, load balancing, and related concerns.
/// </summary>
public class Startup
{
    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpForwarder();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(IApplicationBuilder app, IHttpForwarder forwarder)
    {
        // Configure our own HttpMessageInvoker for outbound calls for proxy operations
        var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            ConnectTimeout = TimeSpan.FromSeconds(15),
        });

        // Setup our own request transform class
        var transformer = new CustomTransformer(); // or HttpTransformer.Default;
        var requestOptions = new ForwarderRequestConfig
        {
            ActivityTimeout = TimeSpan.FromSeconds(100)
        };

        app.UseRouting();

        // When using IHttpForwarder for direct forwarding you are responsible for routing, destination discovery, load balancing, affinity, etc..
        // For an alternate example that includes those features see BasicYarpSample.
        app.UseEndpoints(endpoints =>
        {
            endpoints.Map("/Reports/{**catch-all}", async httpContext =>
            {
                var error = await forwarder.SendAsync(httpContext, "http://localhost", httpClient, requestOptions, QueryStringRequestTransform);

                // Check if the proxy operation was successful
                if (error != ForwarderError.None)
                {
                    var errorFeature = httpContext.Features.Get<IForwarderErrorFeature>();
                    var exception = errorFeature.Exception;
                }
            });

            // When using extension methods for registering IHttpForwarder providing configuration, transforms, and HttpMessageInvoker is optional (defaults will be used).
            endpoints.MapForwarder("/{**catch-all}", "https://example.org", requestOptions, transformer, httpClient);
        });
    }

    private static ValueTask QueryStringRequestTransform(HttpContext context, HttpRequestMessage proxyRequest)
    {
        // Customize the query string:
        var queryContext = new QueryTransformContext(context.Request);
        queryContext.Collection.Remove("param1");
        queryContext.Collection["area"] = "xx2";

        // Assign the custom uri. Be careful about extra slashes when concatenating here. RequestUtilities.MakeDestinationAddress is a safe default.
        proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(
            "http://localhost", 
            context.Request.Path, 
            queryContext.QueryString);

        // Suppress the original request header, use the one from the destination Uri.
        proxyRequest.Headers.Host = null;

        return default;
    }
}