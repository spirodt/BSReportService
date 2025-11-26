using DevExpress.XtraReports.UI;
using BSReportService.BSReports.MFakturi;

namespace BSReportService.Services;

/// <summary>
/// Factory for creating DevExpress report instances based on document type.
/// </summary>
public interface IReportFactory
{
    XtraReport? CreateReport(string documentType);
    IEnumerable<string> GetSupportedDocumentTypes();
}

public class ReportFactory : IReportFactory
{
    private readonly Dictionary<string, Func<XtraReport>> _reportMappings;

    public ReportFactory()
    {
        // Initialize report type mappings
        // Add new report types here as they are created
        _reportMappings = new Dictionary<string, Func<XtraReport>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Ispratnica", () => new frmIspratnica() },
            { "Invoice", () => new DefaultFaktura() },
            { "Faktura", () => new DefaultFaktura() },
            { "DefaultFaktura", () => new DefaultFaktura() },
            // Add more report mappings as needed:
            // { "Nalog", () => new frmNalog() },
        };
    }

    /// <summary>
    /// Creates a report instance based on the document type.
    /// </summary>
    /// <param name="documentType">The type of document to create a report for</param>
    /// <returns>An instance of the appropriate XtraReport, or null if no mapping exists</returns>
    public XtraReport? CreateReport(string documentType)
    {
        if (string.IsNullOrEmpty(documentType))
        {
            return null;
        }

        if (_reportMappings.TryGetValue(documentType, out var reportFactory))
        {
            return reportFactory();
        }

        return null;
    }

    /// <summary>
    /// Gets all supported document types that have report mappings.
    /// </summary>
    public IEnumerable<string> GetSupportedDocumentTypes()
    {
        return _reportMappings.Keys;
    }
}

