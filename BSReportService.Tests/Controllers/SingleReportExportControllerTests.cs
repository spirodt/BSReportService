using BSReportService.Controllers;
using BSReportService.Models;
using BSReportService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace BSReportService.Tests.Controllers;

/// <summary>
/// Unit tests for single report export endpoint.
/// Tests TDD approach - written before implementation.
/// </summary>
public class SingleReportExportControllerTests
{
    private readonly Mock<IReportService> _mockReportService;
    private readonly Mock<ILogger<ReportController>> _mockLogger;
    private readonly ReportController _controller;

    public SingleReportExportControllerTests()
    {
        _mockReportService = new Mock<IReportService>();
        _mockLogger = new Mock<ILogger<ReportController>>();
        _controller = new ReportController(_mockReportService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExportSingleReport_WithValidRequest_ReturnsOkResult()
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
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ExportSingleReportResponse>();
        var response = okResult.Value as ExportSingleReportResponse;
        response!.ReportType.Should().Be("Ispratnica");
        response.PdfContent.Should().NotBeNull();
        response.Status.Should().Be("Success");
    }

    [Fact]
    public async Task ExportSingleReport_WithXmlDataBase64_DecodesAndProcessesCorrectly()
    {
        // Arrange
        var xmlData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><ReportData><DocumentId>INV-001</DocumentId></ReportData>";
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
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockReportService.Verify(x => x.ExportSingleReportToPdfAsync(
            It.Is<ExportSingleReportRequest>(r => r.XmlDataBase64 == xmlBase64),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportSingleReport_WithOutputPath_SavesFileAndReturnsPath()
    {
        // Arrange
        var outputPath = @"C:\Reports\Test_Report.pdf";
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            OutputPath = outputPath
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = new byte[] { 1, 2, 3 },
            SavedFilePath = outputPath,
            PdfSizeBytes = 3,
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.Is<ExportSingleReportRequest>(r => r.OutputPath == outputPath),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as ExportSingleReportResponse;
        response!.SavedFilePath.Should().Be(outputPath);
        response.Status.Should().Be("Success");
    }

    [Fact]
    public async Task ExportSingleReport_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.ExportSingleReport(null!, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("Request is required");
    }

    [Fact]
    public async Task ExportSingleReport_WithEmptyReportType_ReturnsBadRequest()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = string.Empty
        };

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("ReportType is required");
    }

    [Fact]
    public async Task ExportSingleReport_WithInvalidBase64_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = "invalid-base64!@#$%"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid base64 string"));

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task ExportSingleReport_WithCancellationToken_HandlesCancellation()
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

        // Act
        var result = await _controller.ExportSingleReport(request, cts.Token);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(499); // Client Closed Request
    }

    [Fact]
    public async Task ExportSingleReport_WithServiceError_ReturnsInternalServerError()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal service error"));

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task ExportSingleReport_WithBase64Response_ReturnsBase64EncodedPdf()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica"
        };

        var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "Ispratnica",
            PdfContent = pdfBytes,
            PdfContentBase64 = Convert.ToBase64String(pdfBytes),
            PdfSizeBytes = 5,
            Status = "Success"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as ExportSingleReportResponse;
        response!.PdfContentBase64.Should().NotBeNullOrEmpty();
        response.PdfContentBase64.Should().Be(Convert.ToBase64String(pdfBytes));
    }

    [Fact]
    public async Task ExportSingleReport_WithParameters_PassesParametersToService()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "DocumentId", "INV-001" },
            { "CompanyName", "Test Company" },
            { "Date", "2024-01-01" }
        };

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            Parameters = parameters
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
                It.Is<ExportSingleReportRequest>(r => 
                    r.Parameters != null && 
                    r.Parameters.Count == 3 &&
                    r.Parameters["DocumentId"] == "INV-001"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockReportService.Verify(x => x.ExportSingleReportToPdfAsync(
            It.Is<ExportSingleReportRequest>(r => 
                r.Parameters != null && r.Parameters.Count == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportSingleReport_WithInvalidReportType_ReturnsErrorResponse()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "InvalidReportType"
        };

        var expectedResponse = new ExportSingleReportResponse
        {
            ReportType = "InvalidReportType",
            Status = "Error",
            ErrorMessage = "Report type 'InvalidReportType' not found"
        };

        _mockReportService
            .Setup(x => x.ExportSingleReportToPdfAsync(
                It.IsAny<ExportSingleReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExportSingleReport(request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as ExportSingleReportResponse;
        response!.Status.Should().Be("Error");
        response.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}

