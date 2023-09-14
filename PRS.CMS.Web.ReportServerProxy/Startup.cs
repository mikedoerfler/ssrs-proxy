using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Yarp.ReverseProxy.Forwarder;

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
        services.AddYarp();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    // the Microsoft.Owin.Security.DataHandler namespace has the SecureDataFormat and that leads to the
                    // IDataSerializer, IDataProtector classes that know how to create an Owin compatible Auth Cookie
                    options.Cookie.Name = "PRS.CMS.Web.ReportServerProxy.Cookies.Auth";
                    // this would be where our cookie would have to be set to domain so it could
                    // pass between this app and the ReportServer CustomAuth backend
                    //options.Cookie.Domain = ".casemax.com"
                }
            )
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
                options =>
                {
                    options.Authority = "https://localhost:44302/CMSAuth/";

                    options.ClientId = "EEEAF9FC-FDF4-4BB6-9707-F8ED4F3721BD";
                    options.ClientSecret = "secret";
                    options.UsePkce = true;
                    options.SaveTokens = true;

                    /*
                     * these are the default values - if we have more than 1 external IdP then we will
                     * need to set these for each OidClient
                     *
                    options.CallbackPath = "/signin-oidc";
                    options.SignedOutCallbackPath = "signout-callback-oidc";
                    options.RemoteSignOutPath = "signout-oidc";
                    */
                });

    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(IApplicationBuilder app, IHttpForwarder forwarder)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        // for oidc we would add items into here to communicate with CMSAuth to figure out who the end user is

        // When using IHttpForwarder for direct forwarding you are responsible for routing, destination discovery, load balancing, affinity, etc..
        // For an alternate example that includes those features see BasicYarpSample.
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapReverseProxy();
        });
    }
}