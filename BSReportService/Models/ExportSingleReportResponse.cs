namespace BSReportService.Models;

/// <summary>
/// Response model for single report export operation.
/// Contains the generated PDF content and metadata about the export.
/// </summary>
public class ExportSingleReportResponse
{
    /// <summary>
    /// Type of report that was generated.
    /// </summary>
    /// <example>Ispratnica</example>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// The generated PDF content as byte array.
    /// This can be saved to disk or sent directly to the client.
    /// </summary>
    public byte[]? PdfContent { get; set; }

    /// <summary>
    /// Base64-encoded PDF content.
    /// Useful for VBA applications that prefer base64 encoding.
    /// </summary>
    public string? PdfContentBase64 { get; set; }

    /// <summary>
    /// File path where the PDF was saved (if OutputPath was provided in request).
    /// </summary>
    /// <example>C:\Reports\Invoice_001.pdf</example>
    public string? SavedFilePath { get; set; }

    /// <summary>
    /// Size of the generated PDF in bytes.
    /// </summary>
    /// <example>102400</example>
    public long PdfSizeBytes { get; set; }

    /// <summary>
    /// Timestamp when the PDF was generated (UTC).
    /// </summary>
    /// <example>2024-12-14T12:00:00Z</example>
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status of the export operation.
    /// </summary>
    /// <example>Success</example>
    public string Status { get; set; } = "Success";

    /// <summary>
    /// Error message if the export failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

