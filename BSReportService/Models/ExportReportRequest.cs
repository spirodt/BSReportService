namespace BSReportService.Models;

/// <summary>
/// Request model for exporting reports to PDF.
/// Contains filter criteria to select which documents should be exported.
/// </summary>
public class ExportReportRequest
{
    /// <summary>
    /// Filter criteria for selecting documents to export.
    /// All filter properties are optional and can be combined.
    /// </summary>
    public ReportFilter Filter { get; set; } = new();
}
