namespace BSReportService.Models;

/// <summary>
/// Filter criteria for selecting documents to export to PDF.
/// All properties are optional and can be combined for more specific filtering.
/// </summary>
public class ReportFilter
{
    /// <summary>
    /// Filter by document type (e.g., "Invoice", "Report", "Statement").
    /// </summary>
    /// <example>Invoice</example>
    public string? DocumentType { get; set; }

    /// <summary>
    /// Filter documents created on or after this date.
    /// </summary>
    /// <example>2024-01-01T00:00:00Z</example>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter documents created on or before this date.
    /// </summary>
    /// <example>2024-12-31T23:59:59Z</example>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by document status (e.g., "Active", "Pending", "Completed").
    /// </summary>
    /// <example>Active</example>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by specific document IDs. If provided, only documents with matching IDs will be exported.
    /// </summary>
    /// <example>["doc1", "doc2", "doc3"]</example>
    public List<string>? DocumentIds { get; set; }
}
