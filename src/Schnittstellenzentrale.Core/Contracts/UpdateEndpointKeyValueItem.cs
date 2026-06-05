namespace Schnittstellenzentrale.Core.Contracts;

/// <summary>
/// Repräsentiert ein Schlüssel-Wert-Paar für die Aktualisierung von Endpunktinformationen. Diese Klasse wird verwendet, um flexible Aktualisierungen von Endpunktattributen zu ermöglichen, indem sie Schlüssel-Wert-Paare definiert, die die zu aktualisierenden Eigenschaften und deren neue Werte darstellen.
/// </summary>
public class UpdateEndpointKeyValueItem
{
    /// <summary>
    /// Der Schlüssel, der die zu aktualisierende Eigenschaft des Endpunkts angibt. Zum Beispiel könnte dies "Name", "RelativePath", "Method" oder eine andere Eigenschaft sein, die aktualisiert werden soll. Der Schlüssel sollte mit den Eigenschaften des Endpunkts übereinstimmen, um eine korrekte Aktualisierung zu gewährleisten.
    /// </summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>
    /// Der Wert, der die neue Information für die angegebene Eigenschaft des Endpunkts enthält. Der Wert sollte im richtigen Format vorliegen, das für die jeweilige Eigenschaft erforderlich ist. Zum Beispiel könnte dies ein neuer Name, ein neuer relativer Pfad, eine neue HTTP-Methode oder andere relevante Informationen sein, die zur Aktualisierung des Endpunkts benötigt werden.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
