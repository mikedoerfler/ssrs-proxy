/*
var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.GetSection("ReverseProxy");
var routes = config.GetSection("Routes");

builder.Services.AddReverseProxy()
    .LoadFromConfig(config);

var app = builder.Build();
app.UseRouting();
app.MapReverseProxy();

app.Run();
*/

namespace PRS.CMS.Web.ReportServerProxy;

public static class Program
    {
        public static void Main(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

            var host = hostBuilder.Build();
            host.Run();
        }
    }
