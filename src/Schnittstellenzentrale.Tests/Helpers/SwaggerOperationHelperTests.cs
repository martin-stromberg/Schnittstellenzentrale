using Schnittstellenzentrale.Infrastructure.Helpers;
using CoreHttpMethod = Schnittstellenzentrale.Core.Enums.HttpMethod;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Unit-Tests für <see cref="SwaggerOperationHelper"/>.</summary>
public class SwaggerOperationHelperTests
{
    /// <summary>MapHttpMethod bildet alle unterstützten HTTP-Methodennamen auf das korrekte <see cref="CoreHttpMethod"/>-Enum ab.</summary>
    /// <param name="method">Der HTTP-Methodenname aus dem OpenAPI-Dokument.</param>
    /// <param name="expected">Der erwartete Enum-Wert.</param>
    [Theory]
    [InlineData("GET", CoreHttpMethod.GET)]
    [InlineData("POST", CoreHttpMethod.POST)]
    [InlineData("PUT", CoreHttpMethod.PUT)]
    [InlineData("DELETE", CoreHttpMethod.DELETE)]
    [InlineData("PATCH", CoreHttpMethod.PATCH)]
    [InlineData("HEAD", CoreHttpMethod.HEAD)]
    [InlineData("OPTIONS", CoreHttpMethod.OPTIONS)]
    public void MapHttpMethod_BekannteMethoden_GibtKorrektenEnumWert(string method, CoreHttpMethod expected)
    {
        var result = SwaggerOperationHelper.MapHttpMethod(method);

        Assert.Equal(expected, result);
    }

    /// <summary>MapHttpMethod akzeptiert Methodennamen unabhängig von der Groß-/Kleinschreibung.</summary>
    /// <param name="method">Der HTTP-Methodenname in Kleinschreibung oder gemischter Schreibweise.</param>
    /// <param name="expected">Der erwartete Enum-Wert.</param>
    [Theory]
    [InlineData("head", CoreHttpMethod.HEAD)]
    [InlineData("Options", CoreHttpMethod.OPTIONS)]
    public void MapHttpMethod_Kleinschreibung_WirdNormalisiert(string method, CoreHttpMethod expected)
    {
        var result = SwaggerOperationHelper.MapHttpMethod(method);

        Assert.Equal(expected, result);
    }

    /// <summary>MapHttpMethod wirft bei unbekannten Methodennamen eine <see cref="ArgumentOutOfRangeException"/>.</summary>
    /// <param name="method">Ein nicht unterstützter HTTP-Methodenname.</param>
    [Theory]
    [InlineData("TRACE")]
    [InlineData("CONNECT")]
    [InlineData("")]
    public void MapHttpMethod_UnbekannteMethode_WirftArgumentOutOfRangeException(string method)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SwaggerOperationHelper.MapHttpMethod(method));
    }
}
