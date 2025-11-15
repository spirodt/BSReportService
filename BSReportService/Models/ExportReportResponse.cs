namespace BSReportService.Models;

/// <summary>
/// Response model containing the exported PDF documents.
/// Includes the list of documents with their PDF content, total count, and export timestamp.
/// </summary>
public class ExportReportResponse
{
    /// <summary>
    /// List of exported documents with their PDF content.
    /// Each document contains its metadata and generated PDF bytes.
    /// </summary>
    public List<ReportDocument> Documents { get; set; } = new();

    /// <summary>
    /// Total number of documents exported.
    /// This matches the count of items in the Documents list.
    /// </summary>
    /// <example>5</example>
    public int TotalCount { get; set; }

    /// <summary>
    /// Timestamp when the export was completed (UTC).
    /// </summary>
    /// <example>2024-12-14T12:00:00Z</example>
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;
}
