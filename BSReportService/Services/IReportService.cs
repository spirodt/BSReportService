using BSReportService.Models;

namespace BSReportService.Services;

public interface IReportService
{
    Task<ExportReportResponse> ExportReportsToPdfAsync(ReportFilter filter, CancellationToken cancellationToken = default);
}
