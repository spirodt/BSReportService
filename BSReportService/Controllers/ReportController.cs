using BSReportService.Models;
using BSReportService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BSReportService.Controllers;

/// <summary>
/// Controller for exporting multiple documents to PDF with parallel processing support.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Exports multiple documents to PDF based on the provided filter criteria.
    /// </summary>
    /// <remarks>
    /// This endpoint processes multiple documents to PDF simultaneously using parallel processing for improved performance.
    /// 
    /// **Filter Options:**
    /// - Filter by document type (e.g., "Invoice", "Report")
    /// - Filter by date range using StartDate and EndDate
    /// - Filter by status (e.g., "Active", "Pending")
    /// - Filter by specific document IDs
    /// 
    /// All filter properties are optional and can be combined. If no filters are provided, all available documents will be exported.
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "filter": {
    ///     "documentType": "Invoice",
    ///     "startDate": "2024-01-01T00:00:00Z",
    ///     "endDate": "2024-12-31T23:59:59Z",
    ///     "documentIds": ["doc1", "doc2", "doc3"]
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">The export request containing filter criteria. The filter object is required, but all filter properties are optional.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// Returns an ExportReportResponse containing:
    /// - List of exported documents with PDF content
    /// - Total count of exported documents
    /// - Export timestamp
    /// </returns>
    /// <response code="200">Successfully exported documents to PDF. Returns the list of exported documents.</response>
    /// <response code="400">Bad request. The request or filter is null or invalid.</response>
    /// <response code="499">Request was cancelled by the client.</response>
    /// <response code="500">Internal server error occurred during PDF export.</response>
    [HttpPost("export")]
    [ProducesResponseType(typeof(ExportReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<ExportReportResponse>> ExportReports(
        [FromBody] ExportReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || request.Filter == null)
        {
            _logger.LogWarning("Export request or filter is null");
            return BadRequest("Request and filter are required");
        }

        try
        {
            _logger.LogInformation(
                "Starting PDF export with filter: DocumentType={DocumentType}, DocumentIds={DocumentIds}",
                request.Filter.DocumentType,
                request.Filter.DocumentIds?.Count ?? 0);

            var result = await _reportService.ExportReportsToPdfAsync(request.Filter, cancellationToken);

            _logger.LogInformation(
                "PDF export completed successfully. Exported {Count} documents",
                result.TotalCount);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("PDF export was cancelled");
            return StatusCode(StatusCodes.Status499ClientClosedRequest, "Request was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during PDF export");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during PDF export");
        }
    }

    /// <summary>
    /// Exports a single report to PDF with custom file path and XML data support.
    /// </summary>
    /// <remarks>
    /// This endpoint is optimized for VBA applications and supports:
    /// - Custom output file paths
    /// - XML data with base64 encoding
    /// - Report parameters
    /// - Both binary PDF content and base64-encoded PDF in the response
    /// 
    /// **XML Data Format:**
    /// The XML data should be base64-encoded and follow this structure:
    /// ```xml
    /// &lt;?xml version="1.0" encoding="UTF-8"?&gt;
    /// &lt;ReportData&gt;
    ///   &lt;Header&gt;
    ///     &lt;DocumentId&gt;INV-001&lt;/DocumentId&gt;
    ///     &lt;CompanyName&gt;Test Company&lt;/CompanyName&gt;
    ///   &lt;/Header&gt;
    ///   &lt;Items&gt;
    ///     &lt;Item&gt;
    ///       &lt;Name&gt;Product 1&lt;/Name&gt;
    ///       &lt;Quantity&gt;10&lt;/Quantity&gt;
    ///       &lt;Price&gt;100.00&lt;/Price&gt;
    ///     &lt;/Item&gt;
    ///   &lt;/Items&gt;
    /// &lt;/ReportData&gt;
    /// ```
    /// 
    /// **Example Request (with XML data):**
    /// ```json
    /// {
    ///   "reportType": "Ispratnica",
    ///   "outputPath": "C:\\Reports\\Invoice_001.pdf",
    ///   "xmlDataBase64": "PD94bWwgdmVyc2lvbj0iMS4wIj8+CjxSZXBvcnREYXRhPgogIDxEb2N1bWVudElkPklOVi0wMDE8L0RvY3VtZW50SWQ+CjwvUmVwb3J0RGF0YT4="
    /// }
    /// ```
    /// 
    /// **Example Request (with parameters):**
    /// ```json
    /// {
    ///   "reportType": "Ispratnica",
    ///   "outputPath": "C:\\Reports\\Invoice_001.pdf",
    ///   "parameters": {
    ///     "DocumentId": "INV-001",
    ///     "CompanyName": "Test Company"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">The export request containing report type, data, and output path.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// Returns an ExportSingleReportResponse containing:
    /// - Generated PDF content (both binary and base64-encoded)
    /// - File path if saved to disk
    /// - PDF size and generation timestamp
    /// </returns>
    /// <response code="200">Successfully exported the report to PDF. Returns the PDF content and metadata.</response>
    /// <response code="400">Bad request. The request is null or invalid (e.g., missing ReportType).</response>
    /// <response code="499">Request was cancelled by the client.</response>
    /// <response code="500">Internal server error occurred during PDF export.</response>
    [HttpPost("export-single")]
    [ProducesResponseType(typeof(ExportSingleReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<ActionResult<ExportSingleReportResponse>> ExportSingleReport(
        [FromBody] ExportSingleReportRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            _logger.LogWarning("Export single report request is null");
            return BadRequest("Request is required");
        }

        if (string.IsNullOrEmpty(request.ReportType))
        {
            _logger.LogWarning("ReportType is missing in request");
            return BadRequest("ReportType is required");
        }

        try
        {
            _logger.LogInformation(
                "Starting single PDF export for ReportType={ReportType}, OutputPath={OutputPath}",
                request.ReportType,
                request.OutputPath ?? "none");

            var result = await _reportService.ExportSingleReportToPdfAsync(request, cancellationToken);

            if (result.Status == "Error")
            {
                _logger.LogWarning(
                    "Single PDF export completed with error: {ErrorMessage}",
                    result.ErrorMessage);
            }
            else
            {
                _logger.LogInformation(
                    "Single PDF export completed successfully. Size: {Size} bytes",
                    result.PdfSizeBytes);
            }

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Single PDF export was cancelled");
            return StatusCode(StatusCodes.Status499ClientClosedRequest, "Request was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during single PDF export");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during PDF export");
        }
    }
}
