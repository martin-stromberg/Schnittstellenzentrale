using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.ModelBuilder;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.OData;

/// <summary>Erstellt das OData-EDM-Modell mit den vier Entity-Types und Navigationseigenschaften.</summary>
public static class ODataEdmModelBuilder
{
    private const string SzAuthTypeTerm = "x-sz-auth-type";

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

        var authenticate = builder.Action("Authenticate");
        authenticate.Returns<AuthenticateResponse>();

        var model = (EdmModel)builder.GetEdmModel();

        var authTypeTerm = new EdmTerm("Schnittstellenzentrale.V1", SzAuthTypeTerm, EdmCoreModel.Instance.GetString(true));
        model.AddElement(authTypeTerm);

        foreach (var entitySetName in new[] { "Applications", "ApplicationGroups", "Endpoints", "EndpointGroups" })
        {
            var entitySet = model.EntityContainer?.FindEntitySet(entitySetName);
            if (entitySet != null)
                model.SetVocabularyAnnotation(new EdmVocabularyAnnotation(entitySet, authTypeTerm, new EdmStringConstant("BearerToken")));
        }

        var authenticateAction = model.SchemaElements
            .OfType<IEdmAction>()
            .FirstOrDefault(a => a.Name == "Authenticate");
        if (authenticateAction != null)
            model.SetVocabularyAnnotation(new EdmVocabularyAnnotation(authenticateAction, authTypeTerm, new EdmStringConstant("Negotiate")));

        return model;
    }
}
