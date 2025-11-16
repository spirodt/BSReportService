using BSReportService.Models;

namespace BSReportService.Services;

public interface IReportService
{
    Task<ExportReportResponse> ExportReportsToPdfAsync(ReportFilter filter, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Exports a single report to PDF with custom file path and XML data support.
    /// </summary>
    /// <param name="request">The export request containing report type, data, and output path.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The export response containing PDF content and metadata.</returns>
    Task<ExportSingleReportResponse> ExportSingleReportToPdfAsync(ExportSingleReportRequest request, CancellationToken cancellationToken = default);
}
