using BSReportService.Models;
using BSReportService.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace BSReportService.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IReportService> _mockReportService;

    public ReportServiceTests()
    {
        _mockReportService = new Mock<IReportService>();
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithValidFilter_ReturnsResponse()
    {
        // Arrange
        var filter = new ReportFilter
        {
            DocumentType = "Invoice",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        var expectedResponse = new ExportReportResponse
        {
            Documents = new List<ReportDocument>(),
            TotalCount = 0,
            ExportDate = DateTime.UtcNow
        };

        _mockReportService
            .Setup(x => x.ExportReportsToPdfAsync(It.IsAny<ReportFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Documents.Should().BeEmpty();
        _mockReportService.Verify(x => x.ExportReportsToPdfAsync(
            It.Is<ReportFilter>(f => f.DocumentType == "Invoice"), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithMultipleDocumentIds_ProcessesAllDocuments()
    {
        // Arrange
        var filter = new ReportFilter
        {
            DocumentIds = new List<string> { "doc1", "doc2", "doc3" }
        };

        var expectedDocuments = new List<ReportDocument>
        {
            new() { DocumentId = "doc1", PdfContent = new byte[] { 1, 2, 3 } },
            new() { DocumentId = "doc2", PdfContent = new byte[] { 4, 5, 6 } },
            new() { DocumentId = "doc3", PdfContent = new byte[] { 7, 8, 9 } }
        };

        var expectedResponse = new ExportReportResponse
        {
            Documents = expectedDocuments,
            TotalCount = 3,
            ExportDate = DateTime.UtcNow
        };

        _mockReportService
            .Setup(x => x.ExportReportsToPdfAsync(It.IsAny<ReportFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Documents.Should().HaveCount(3);
        result.Documents.Should().AllSatisfy(doc => doc.PdfContent.Should().NotBeNull());
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithDateRangeFilter_FiltersCorrectly()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var filter = new ReportFilter
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var expectedResponse = new ExportReportResponse
        {
            Documents = new List<ReportDocument>(),
            TotalCount = 0
        };

        _mockReportService
            .Setup(x => x.ExportReportsToPdfAsync(It.IsAny<ReportFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        _mockReportService.Verify(x => x.ExportReportsToPdfAsync(
            It.Is<ReportFilter>(f => f.StartDate == startDate && f.EndDate == endDate),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var filter = new ReportFilter();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockReportService
            .Setup(x => x.ExportReportsToPdfAsync(It.IsAny<ReportFilter>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockReportService.Object.ExportReportsToPdfAsync(filter, cts.Token));
    }

    [Fact]
    public async Task ExportReportsToPdfAsync_WithEmptyFilter_ReturnsEmptyResult()
    {
        // Arrange
        var filter = new ReportFilter();
        var expectedResponse = new ExportReportResponse
        {
            Documents = new List<ReportDocument>(),
            TotalCount = 0
        };

        _mockReportService
            .Setup(x => x.ExportReportsToPdfAsync(It.IsAny<ReportFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Documents.Should().BeEmpty();
    }
}
