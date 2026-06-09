using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Repositories;

internal static class ModelUpdateExtensions
{
    internal static void ApplyUpdate(this ApplicationGroup existing, ApplicationGroup source)
    {
        existing.Name = source.Name;
        existing.Description = source.Description;
        existing.Subtitle = source.Subtitle;
        existing.IconData = source.IconData;
    }

    internal static void ApplyUpdate(this Application existing, Application source)
    {
        existing.Name = source.Name;
        existing.Description = source.Description;
        existing.BaseUrl = source.BaseUrl;
        existing.InterfaceUrl = source.InterfaceUrl;
        existing.InterfaceType = source.InterfaceType;
        existing.Owner = source.Owner;
        existing.ApplicationGroupId = source.ApplicationGroupId;
    }

    internal static void ApplyUpdate(this EndpointGroup existing, EndpointGroup source)
    {
        existing.Name = source.Name;
        existing.ParentGroupId = source.ParentGroupId;
    }

    internal static void ApplyUpdate(this Endpoint existing, Endpoint source)
    {
        existing.Name = source.Name;
        existing.Method = source.Method;
        existing.RelativePath = source.RelativePath;
        existing.Body = source.Body;
        existing.BodyMode = source.BodyMode;
        existing.AuthenticationType = source.AuthenticationType;
        existing.EndpointGroupId = source.EndpointGroupId;
        existing.PreRequestScript = source.PreRequestScript;
        existing.PostRequestScript = source.PostRequestScript;
    }
}
