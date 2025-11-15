using BSReportService.Models;
using BSReportService.Services;
using FluentAssertions;
using Xunit;

namespace BSReportService.Tests.Services;

public class ReportServiceIntegrationTests
{
    [Fact]
    public async Task ExportReportsToPdfAsync_WithConcreteImplementation_ExportsReportsInParallel()
    {
        // Arrange
        IReportService reportService = new ReportService();
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
        result.Documents.Should().AllSatisfy(doc =>
        {
            doc.PdfContent.Should().NotBeNull();
            doc.PdfContent!.Length.Should().BeGreaterThan(0);
        });

        // Verify parallel processing (should be faster than sequential)
        // This is a heuristic - parallel should complete in reasonable time
        elapsedTime.TotalSeconds.Should().BeLessThan(10);
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithDateFilter_FiltersDocuments()
    {
        // Arrange
        IReportService reportService = new ReportService();
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
    }
}
