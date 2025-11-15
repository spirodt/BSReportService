namespace BSReportService.Models;

/// <summary>
/// Represents a document that has been exported to PDF.
/// Contains document metadata and the generated PDF content as byte array.
/// </summary>
public class ReportDocument
{
    /// <summary>
    /// Unique identifier for the document.
    /// </summary>
    /// <example>doc123</example>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Type of the document (e.g., "Invoice", "Report", "Statement").
    /// </summary>
    /// <example>Invoice</example>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the document (e.g., "Active", "Pending", "Completed").
    /// </summary>
    /// <example>Active</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the document was originally created (UTC).
    /// </summary>
    /// <example>2024-01-15T10:00:00Z</example>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// PDF content of the document as a byte array.
    /// This is the actual PDF file content that can be saved to disk or returned to clients.
    /// </summary>
    /// <example>JVBERi0xLjQKJcOkw7zDtsOgw6jDr8O3w6A...</example>
    public byte[]? PdfContent { get; set; }
}
