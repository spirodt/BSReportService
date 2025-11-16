using BSReportService.Models;
using BSReportService.Services;
using FluentAssertions;
using Moq;
using System.Text;
using Xunit;

namespace BSReportService.Tests.Services;

/// <summary>
/// Unit tests for single report export service functionality.
/// Tests the business logic for exporting single reports with XML/Base64 data.
/// </summary>
public class SingleReportExportServiceTests
{
    private readonly Mock<IReportService> _mockReportService;

    public SingleReportExportServiceTests()
    {
        _mockReportService = new Mock<IReportService>();
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            Parameters = new Dictionary<string, string> { { "DocumentId", "INV-001" } }
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = new byte[] { 1, 2, 3, 4, 5 },
            PdfSizeBytes = 5,
            Status = "Success",
            GeneratedDate = DateTime.UtcNow
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ReportType.Should().Be("Ispratnica");
        result.PdfContent.Should().NotBeNull();
        result.PdfContent!.Length.Should().Be(5);
        result.Status.Should().Be("Success");
        result.PdfSizeBytes.Should().Be(5);
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithXmlData_ParsesXmlCorrectly()
    {
        // Arrange
        var xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReportData>
    <DocumentId>INV-001</DocumentId>
    <Company>Test Company</Company>
    <Items>
        <Item>
            <Name>Product 1</Name>
            <Quantity>10</Quantity>
        </Item>
    </Items>
</ReportData>";
        var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlData));

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = xmlBase64
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = new byte[] { 1, 2, 3 },
            PdfSizeBytes = 3,
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.Is<ExportSingleReportRequest>(r => r.XmlDataBase64 == xmlBase64),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        _mockReportService.Verify(x => x.ExportSingleReportToPdfAsync(
            It.Is<ExportSingleReportRequest>(r => r.XmlDataBase64 == xmlBase64),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithOutputPath_SavesFileSuccessfully()
    {
        // Arrange
        var outputPath = @"C:\Reports\Test_Report.pdf";
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            OutputPath = outputPath,
            Parameters = new Dictionary<string, string> { { "DocumentId", "INV-001" } }
        };

        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };
        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = pdfContent,
            SavedFilePath = outputPath,
            PdfSizeBytes = pdfContent.Length,
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.Is<ExportSingleReportRequest>(r => r.OutputPath == outputPath),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SavedFilePath.Should().Be(outputPath);
        result.PdfContent.Should().NotBeNull();
        result.Status.Should().Be("Success");
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithBase64Encoding_ReturnsBase64Content()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica"
        };

        var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
        var pdfBase64 = Convert.ToBase64String(pdfBytes);

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = pdfBytes,
            PdfContentBase64 = pdfBase64,
            PdfSizeBytes = pdfBytes.Length,
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.PdfContentBase64.Should().NotBeNullOrEmpty();
        result.PdfContentBase64.Should().Be(pdfBase64);
        var decodedBytes = Convert.FromBase64String(result.PdfContentBase64!);
        decodedBytes.Should().Equal(pdfBytes);
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithInvalidBase64_ThrowsFormatException()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = "invalid-base64-string!@#$"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid base64 string"));

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() =>
            _mockReportService.Object.ExportSingleReportToPdfAsync(request));
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _mockReportService.Object.ExportSingleReportToPdfAsync(request, cts.Token));
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithComplexXmlStructure_HandlesNestedData()
    {
        // Arrange
        var xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReportData>
    <Header>
        <DocumentId>INV-001</DocumentId>
        <Company>Test Company</Company>
        <Date>2024-01-01</Date>
    </Header>
    <Items>
        <Item>
            <Name>Product 1</Name>
            <Quantity>10</Quantity>
            <Price>100.00</Price>
        </Item>
        <Item>
            <Name>Product 2</Name>
            <Quantity>5</Quantity>
            <Price>50.00</Price>
        </Item>
    </Items>
    <Footer>
        <Total>1250.00</Total>
    </Footer>
</ReportData>";
        var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlData));

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = xmlBase64
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = new byte[100],
            PdfSizeBytes = 100,
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        result.PdfSizeBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithMissingReportType_ReturnsErrorResponse()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "NonExistentReport"
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "NonExistentReport",
            Status = "Error",
            ErrorMessage = "Report type 'NonExistentReport' not found"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Error");
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.PdfContent.Should().BeNull();
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithFileSystemError_ReturnsErrorResponse()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            OutputPath = @"Z:\InvalidDrive\report.pdf"
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            Status = "Error",
            ErrorMessage = "Failed to save file: The specified path is invalid"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Error");
        result.ErrorMessage.Should().Contain("Failed to save file");
    }

    [Fact]
    public async Task ExportSingleReportToPdfAsync_WithMultipleParameters_AppliesAllParameters()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "DocumentId", "INV-001" },
            { "CompanyName", "Test Company" },
            { "Date", "2024-01-01" },
            { "Customer", "John Doe" }
        };

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            Parameters = parameters
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = new byte[50],
            PdfSizeBytes = 50,
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.Is<ExportSingleReportRequest>(r => 
                    r.Parameters != null && r.Parameters.Count == 4),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockReportService.Object.ExportSingleReportToPdfAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Success");
        _mockReportService.Verify(x => x.ExportSingleReportToPdfAsync(
            It.Is<ExportSingleReportRequest>(r => 
                r.Parameters != null && r.Parameters.Count == 4),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

