using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.OData;

/// <summary>Erstellt das OData-EDM-Modell mit den vier Entity-Types und Navigationseigenschaften.</summary>
public static class ODataEdmModelBuilder
{
    /// <summary>Erzeugt und gibt das fertige <see cref="IEdmModel"/> zurück.</summary>
    public static IEdmModel Build()
    {
        var builder = new ODataConventionModelBuilder();

        var applications = builder.EntitySet<Application>("Applications");
        applications.EntityType.HasKey(a => a.Id);
        applications.EntityType.Ignore(a => a.Links);

        var applicationGroups = builder.EntitySet<ApplicationGroup>("ApplicationGroups");
        applicationGroups.EntityType.HasKey(g => g.Id);

        var endpoints = builder.EntitySet<Core.Models.Endpoint>("Endpoints");
        endpoints.EntityType.HasKey(e => e.Id);
        endpoints.EntityType.Ignore(e => e.Headers);
        endpoints.EntityType.Ignore(e => e.QueryParameters);

        var endpointGroups = builder.EntitySet<EndpointGroup>("EndpointGroups");
        endpointGroups.EntityType.HasKey(g => g.Id);

        return builder.GetEdmModel();
    }
}
