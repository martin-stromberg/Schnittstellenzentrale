namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Konfigurationsklasse für Upload-Einstellungen.</summary>
public class UploadSettings
{
    /// <summary>Maximale Icon-Dateigröße in Bytes.</summary>
    public int MaxIconSizeBytes { get; set; } = 524288;
}
