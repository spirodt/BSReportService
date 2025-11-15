using BSReportService.Models;

namespace BSReportService.Services;

public class ReportService : IReportService
{
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

        var allDocuments = GenerateDummyDocuments();

        var filteredDocuments = allDocuments.AsQueryable();

        if (!string.IsNullOrEmpty(filter.DocumentType))
        {
            filteredDocuments = filteredDocuments.Where(d => d.DocumentType == filter.DocumentType);
        }

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

        if (filter.DocumentIds != null && filter.DocumentIds.Any())
        {
            filteredDocuments = filteredDocuments.Where(d => filter.DocumentIds.Contains(d.DocumentId));
        }

        return filteredDocuments.ToList();
    }

    private async Task<ReportDocument> ExportDocumentToPdfAsync(ReportDocument document, CancellationToken cancellationToken)
    {
        // Dummy PDF export implementation
        // In real scenario, this would generate actual PDF using a library like iTextSharp, PdfSharp, etc.
        
        // Simulate PDF generation time (varies per document)
        var delayMs = Random.Shared.Next(100, 500);
        await Task.Delay(delayMs, cancellationToken);

        // Generate dummy PDF content (in real scenario, this would be actual PDF bytes)
        var pdfContent = GenerateDummyPdfContent(document);

        return new ReportDocument
        {
            DocumentId = document.DocumentId,
            DocumentType = document.DocumentType,
            Status = document.Status,
            CreatedDate = document.CreatedDate,
            PdfContent = pdfContent
        };
    }

    private byte[] GenerateDummyPdfContent(ReportDocument document)
    {
        // Dummy PDF content - in real implementation, this would generate actual PDF
        // For now, return a simple byte array that represents PDF structure
        var content = $"PDF content for document {document.DocumentId} of type {document.DocumentType}";
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    private List<ReportDocument> GenerateDummyDocuments()
    {
        // Generate dummy documents for testing
        var documents = new List<ReportDocument>();

        for (int i = 1; i <= 10; i++)
        {
            documents.Add(new ReportDocument
            {
                DocumentId = $"doc{i}",
                DocumentType = i % 2 == 0 ? "Invoice" : "Report",
                Status = i % 3 == 0 ? "Active" : "Pending",
                CreatedDate = DateTime.UtcNow.AddDays(-i)
            });
        }

        return documents;
    }
}
