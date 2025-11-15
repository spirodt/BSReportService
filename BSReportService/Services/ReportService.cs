using BSReportService.Models;
using BSReportService.BSReports;
using DevExpress.XtraReports.UI;
using System.IO;

namespace BSReportService.Services;

public class ReportService : IReportService
{
    private readonly IReportFactory _reportFactory;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IReportFactory reportFactory, ILogger<ReportService> logger)
    {
        _reportFactory = reportFactory;
        _logger = logger;
    }

    public async Task<ExportReportResponse> ExportReportsToPdfAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        // Get documents based on filter (dummy implementation)
        var documents = await GetDocumentsByFilterAsync(filter, cancellationToken);

        // Export all documents to PDF in parallel
        var exportTasks = documents.Select(doc => ExportDocumentToPdfAsync(doc, cancellationToken));
        var exportedDocuments = await Task.WhenAll(exportTasks);

        return new ExportReportResponse
        {
            Documents = exportedDocuments.ToList(),
            TotalCount = exportedDocuments.Length,
            ExportDate = DateTime.UtcNow
        };
    }

    private async Task<List<ReportDocument>> GetDocumentsByFilterAsync(ReportFilter filter, CancellationToken cancellationToken)
    {
        // Dummy implementation - in real scenario, this would query a database or data source
        await Task.Delay(10, cancellationToken); // Simulate async operation

        // Generate documents based on whether specific IDs are requested and the document type
        var allDocuments = GenerateDummyDocuments(filter.DocumentIds, filter.DocumentType);

        var filteredDocuments = allDocuments.AsQueryable();

        if (filter.StartDate.HasValue)
        {
            filteredDocuments = filteredDocuments.Where(d => d.CreatedDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            filteredDocuments = filteredDocuments.Where(d => d.CreatedDate <= filter.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            filteredDocuments = filteredDocuments.Where(d => d.Status == filter.Status);
        }

        return filteredDocuments.ToList();
    }

    private async Task<ReportDocument> ExportDocumentToPdfAsync(ReportDocument document, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generating PDF for document {DocumentId} of type {DocumentType}", 
                document.DocumentId, document.DocumentType);

            // Create the appropriate report based on document type
            var report = _reportFactory.CreateReport(document.DocumentType);
            
            if (report == null)
            {
                _logger.LogWarning("No report template found for document type {DocumentType}. Using fallback.", 
                    document.DocumentType);
                
                // Return document with null PDF content if no report template exists
                return new ReportDocument
                {
                    DocumentId = document.DocumentId,
                    DocumentType = document.DocumentType,
                    Status = "Error: No report template",
                    CreatedDate = document.CreatedDate,
                    PdfContent = null
                };
            }

            // Set report parameters if the report uses them
            // For example, frmIspratnica has a "Faktura" parameter
            if (report.Parameters.Count > 0)
            {
                // Try to set the first parameter with the document ID
                // This is a simple example - in production, you'd map specific parameters
                var firstParam = report.Parameters[0];
                firstParam.Value = document.DocumentId;
            }

            // Set the data source for the report
            // In a real scenario, this would be fetched from a database
            var reportData = await GetReportDataAsync(document, cancellationToken);
            report.DataSource = reportData;

            // Generate PDF content using DevExpress export
            byte[] pdfContent;
            using (var memoryStream = new MemoryStream())
            {
                // Execute the report to bind data
                await Task.Run(() => report.CreateDocument(), cancellationToken);
                
                // Export to PDF
                await Task.Run(() => report.ExportToPdf(memoryStream), cancellationToken);
                
                pdfContent = memoryStream.ToArray();
            }

            _logger.LogInformation("Successfully generated PDF for document {DocumentId}, size: {Size} bytes", 
                document.DocumentId, pdfContent.Length);

            return new ReportDocument
            {
                DocumentId = document.DocumentId,
                DocumentType = document.DocumentType,
                Status = document.Status,
                CreatedDate = document.CreatedDate,
                PdfContent = pdfContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for document {DocumentId}", document.DocumentId);
            
            return new ReportDocument
            {
                DocumentId = document.DocumentId,
                DocumentType = document.DocumentType,
                Status = $"Error: {ex.Message}",
                CreatedDate = document.CreatedDate,
                PdfContent = null
            };
        }
    }

    private async Task<object> GetReportDataAsync(ReportDocument document, CancellationToken cancellationToken)
    {
        // This is where you would fetch actual data from your database
        // For now, return dummy data structure that matches the report's expected schema
        
        await Task.Delay(10, cancellationToken); // Simulate async database call

        // Use the helper to create data based on document type
        return document.DocumentType.ToLowerInvariant() switch
        {
            "ispratnica" => ReportDataHelper.CreateIspratnicaData(document.DocumentId, document.CreatedDate),
            "invoice" => ReportDataHelper.CreateIspratnicaData(document.DocumentId, document.CreatedDate),
            _ => ReportDataHelper.CreateCustomReportData(document.DocumentId, document.CreatedDate)
        };
    }

    private List<ReportDocument> GenerateDummyDocuments(List<string>? specificIds = null, string? documentType = null)
    {
        // Generate dummy documents for testing
        var documents = new List<ReportDocument>();
        var supportedTypes = _reportFactory.GetSupportedDocumentTypes().ToList();

        // Determine which document type to use
        string GetDocumentType(int index)
        {
            if (!string.IsNullOrEmpty(documentType))
            {
                return documentType; // Use the specified document type
            }
            
            if (supportedTypes.Count > 0)
            {
                return supportedTypes[index % supportedTypes.Count];
            }
            
            return "Unknown";
        }

        // If specific IDs are provided, generate only those documents
        if (specificIds != null && specificIds.Any())
        {
            for (int i = 0; i < specificIds.Count; i++)
            {
                documents.Add(new ReportDocument
                {
                    DocumentId = specificIds[i],
                    DocumentType = GetDocumentType(i),
                    Status = i % 3 == 0 ? "Active" : "Pending",
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                });
            }
        }
        else
        {
            // Generate default set of documents
            for (int i = 1; i <= 10; i++)
            {
                documents.Add(new ReportDocument
                {
                    DocumentId = $"doc{i}",
                    DocumentType = GetDocumentType(i),
                    Status = i % 3 == 0 ? "Active" : "Pending",
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                });
            }
        }

        return documents;
    }
}
