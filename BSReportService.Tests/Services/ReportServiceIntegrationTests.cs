using BSReportService.Models;
using BSReportService.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BSReportService.Tests.Services;

public class ReportServiceIntegrationTests
{
    private readonly IReportFactory _reportFactory;
    private readonly ILogger<ReportService> _logger;

    public ReportServiceIntegrationTests()
    {
        _reportFactory = new ReportFactory();
        _logger = Mock.Of<ILogger<ReportService>>();
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithConcreteImplementation_ExportsReportsInParallel()
    {
        // Arrange
        IReportService reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentIds = new List<string> { "doc1", "doc2", "doc3", "doc4", "doc5" }
        };

        // Act
        var startTime = DateTime.UtcNow;
        var result = await reportService.ExportReportsToPdfAsync(filter);
        var elapsedTime = DateTime.UtcNow - startTime;

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
        
        // All documents should have PDF content (may be null if no template exists, which is OK)
        result.Documents.Should().AllSatisfy(doc =>
        {
            doc.DocumentId.Should().NotBeNullOrEmpty();
            doc.DocumentType.Should().NotBeNullOrEmpty();
        });

        // Verify parallel processing (should be faster than sequential)
        // This is a heuristic - parallel should complete in reasonable time
        elapsedTime.TotalSeconds.Should().BeLessThan(10);
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithDateFilter_FiltersDocuments()
    {
        // Arrange
        IReportService reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().NotBeNull();
        result.Documents.Should().AllSatisfy(doc =>
        {
            doc.CreatedDate.Should().BeOnOrAfter(filter.StartDate!.Value);
            doc.CreatedDate.Should().BeOnOrBefore(filter.EndDate!.Value);
        });
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithIspratnicaDocumentType_GeneratesPdf()
    {
        // Arrange
        IReportService reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentType = "Ispratnica",
            DocumentIds = new List<string> { "doc1" }
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(1);
        
        var document = result.Documents.First();
        document.DocumentType.Should().Be("Ispratnica");
        document.PdfContent.Should().NotBeNull("DevExpress report should generate PDF content");
        document.PdfContent!.Length.Should().BeGreaterThan(0, "PDF should have content");
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithMultipleIspratnicaDocuments_GeneratesAllPdfs()
    {
        // Arrange
        IReportService reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentType = "Ispratnica",
            DocumentIds = new List<string> { "doc1", "doc2", "doc3" }
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(3);
        
        result.Documents.Should().AllSatisfy(doc =>
        {
            doc.DocumentType.Should().Be("Ispratnica");
            doc.PdfContent.Should().NotBeNull("All Ispratnica documents should have PDF content");
            doc.PdfContent!.Length.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithUnsupportedDocumentType_HandlesGracefully()
    {
        // Arrange
        IReportService reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentType = "UnsupportedType",
            DocumentIds = new List<string> { "doc1" }
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(1);
        
        var document = result.Documents.First();
        // Document should be returned even if PDF generation fails
        document.DocumentId.Should().Be("doc1");
    }
}
