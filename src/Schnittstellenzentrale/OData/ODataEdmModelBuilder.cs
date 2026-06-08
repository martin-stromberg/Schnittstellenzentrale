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
    private const string SzPostRequestScriptTerm = "x-sz-post-request-script";
    private const string SzBearerTokenTerm = "x-sz-bearer-token";
    private const string AuthenticateScript = "sz.environment.set('schnittstellenzentrale.authToken', sz.response.body.asJson().token);";
    private const string EntitySetScript = "var headerName = 'X-New-Token'; var newToken = sz.response.headers[headerName]; sz.environment.set('schnittstellenzentrale.authToken', newToken);";
    private const string BearerTokenVariable = "{{schnittstellenzentrale.authToken}}";

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

        var postScriptTerm = new EdmTerm("Schnittstellenzentrale.V1", SzPostRequestScriptTerm, EdmCoreModel.Instance.GetString(true));
        model.AddElement(postScriptTerm);

        var bearerTokenTerm = new EdmTerm("Schnittstellenzentrale.V1", SzBearerTokenTerm, EdmCoreModel.Instance.GetString(true));
        model.AddElement(bearerTokenTerm);

        foreach (var entitySetName in new[] { "Applications", "ApplicationGroups", "Endpoints", "EndpointGroups" })
        {
            var entitySet = model.EntityContainer?.FindEntitySet(entitySetName);
            if (entitySet != null)
            {
                model.SetVocabularyAnnotation(new EdmVocabularyAnnotation(entitySet, authTypeTerm, new EdmStringConstant("BearerToken")));
                model.SetVocabularyAnnotation(new EdmVocabularyAnnotation(entitySet, postScriptTerm, new EdmStringConstant(EntitySetScript)));
                model.SetVocabularyAnnotation(new EdmVocabularyAnnotation(entitySet, bearerTokenTerm, new EdmStringConstant(BearerTokenVariable)));
            }
        }

        var authenticateAction = model.SchemaElements
            .OfType<IEdmAction>()
            .FirstOrDefault(a => a.Name == "Authenticate");
        if (authenticateAction != null)
        {
            model.SetVocabularyAnnotation(new EdmVocabularyAnnotation(authenticateAction, authTypeTerm, new EdmStringConstant("Negotiate")));
            model.SetVocabularyAnnotation(new EdmVocabularyAnnotation(authenticateAction, postScriptTerm, new EdmStringConstant(AuthenticateScript)));
        }

        return model;
    }
}
