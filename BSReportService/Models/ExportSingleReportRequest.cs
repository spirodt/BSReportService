namespace BSReportService.Models;

/// <summary>
/// Request model for exporting a single report to PDF with custom file path.
/// Supports XML data input with base64 encoding for VBA integration.
/// </summary>
public class ExportSingleReportRequest
{
    /// <summary>
    /// The type of report to generate (e.g., "Ispratnica", "Invoice").
    /// This determines which DevExpress report template will be used.
    /// </summary>
    /// <example>Ispratnica</example>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Optional file path where the PDF should be saved.
    /// If not provided, the PDF will only be returned in the response.
    /// </summary>
    /// <example>C:\Reports\Invoice_001.pdf</example>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Base64-encoded XML data containing the report data.
    /// This allows VBA applications to send structured data securely.
    /// The XML should match the expected schema for the specific report type.
    /// </summary>
    /// <example>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPFJlcG9ydERhdGE+PC9SZXBvcnREYXRhPg==</example>
    public string? XmlDataBase64 { get; set; }

    /// <summary>
    /// Optional parameters for the report (e.g., document ID, filter values).
    /// </summary>
    public Dictionary<string, string>? Parameters { get; set; }
}

