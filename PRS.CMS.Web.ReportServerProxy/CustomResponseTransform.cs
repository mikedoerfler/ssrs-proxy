using Yarp.ReverseProxy.Transforms;

namespace PRS.CMS.Web.ReportServerProxy;

public class CustomResponseTransform : ResponseTransform
{
    public override ValueTask ApplyAsync(ResponseTransformContext context)
    {
        return ValueTask.CompletedTask;
    }
}