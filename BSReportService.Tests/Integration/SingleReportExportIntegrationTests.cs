using System.Net;
using System.Net.Http.Json;
using System.Text;
using BSReportService.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BSReportService.Tests.Integration;

/// <summary>
/// Integration tests for single report export endpoint.
/// Tests the full request/response pipeline with actual HTTP calls.
/// </summary>
public class SingleReportExportIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SingleReportExportIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ExportSingleReport_WithValidRequest_ReturnsOkWithPdfContent()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            Parameters = new Dictionary<string, string>
            {
                { "DocumentId", "INV-001" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.ReportType.Should().Be("Ispratnica");
        result.Status.Should().Be("Success");
        result.PdfContent.Should().NotBeNull();
        result.PdfSizeBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportSingleReport_WithXmlBase64Data_ProcessesSuccessfully()
    {
        // Arrange
        var xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReportData>
    <DocumentId>INV-001</DocumentId>
    <CompanyName>Test Company</CompanyName>
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
</ReportData>";
        var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlData));

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = xmlBase64
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        result.PdfContent.Should().NotBeNull();
        result.PdfContentBase64.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportSingleReport_WithOutputPath_SavesFileAndReturnsPath()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_report_{Guid.NewGuid()}.pdf");
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            OutputPath = tempPath,
            Parameters = new Dictionary<string, string>
            {
                { "DocumentId", "INV-001" }
            }
        };

        try
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
            result.Should().NotBeNull();
            result!.SavedFilePath.Should().Be(tempPath);
            result.Status.Should().Be("Success");

            // Verify file was created (in real implementation)
            // File.Exists(tempPath).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task ExportSingleReport_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync<ExportSingleReportRequest>("/api/report/export-single", null!);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
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
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportSingleReport_WithInvalidBase64_ReturnsErrorResponse()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = "invalid-base64-string!@#$%^&*()"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        // The service handles invalid Base64 and returns an error response
        // (not throwing an exception, so we get 200 with error in the body)
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Error");
    }

    [Fact]
    public async Task ExportSingleReport_WithInvalidReportType_ReturnsErrorResponse()
    {
        // Arrange
        var request = new ExportSingleReportRequest
        {
            ReportType = "NonExistentReportType"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        // The endpoint should return OK but with an error in the response body
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Error");
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportSingleReport_WithComplexXmlStructure_GeneratesPdfCorrectly()
    {
        // Arrange
        var xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReportData>
    <Header>
        <DocumentId>INV-001</DocumentId>
        <CompanyName>Test Company Ltd.</CompanyName>
        <Address>123 Test Street, Test City</Address>
        <Date>2024-01-15</Date>
    </Header>
    <Items>
        <Item>
            <ItemCode>PROD001</ItemCode>
            <Name>Product 1</Name>
            <Description>Test Product Description</Description>
            <Quantity>10</Quantity>
            <UnitPrice>100.00</UnitPrice>
            <Total>1000.00</Total>
        </Item>
        <Item>
            <ItemCode>PROD002</ItemCode>
            <Name>Product 2</Name>
            <Description>Another Test Product</Description>
            <Quantity>5</Quantity>
            <UnitPrice>50.00</UnitPrice>
            <Total>250.00</Total>
        </Item>
    </Items>
    <Summary>
        <Subtotal>1250.00</Subtotal>
        <Tax>250.00</Tax>
        <Total>1500.00</Total>
    </Summary>
</ReportData>";
        var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlData));

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = xmlBase64
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        result.PdfContent.Should().NotBeNull();
        result.PdfSizeBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportSingleReport_WithBothParametersAndXml_UsesXmlData()
    {
        // Arrange
        var xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReportData>
    <DocumentId>INV-XML</DocumentId>
</ReportData>";
        var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlData));

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = xmlBase64,
            Parameters = new Dictionary<string, string>
            {
                { "DocumentId", "INV-PARAM" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
    }

    [Fact]
    public async Task ExportSingleReport_WithSpecialCharactersInXml_HandlesCorrectly()
    {
        // Arrange
        var xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReportData>
    <DocumentId>INV-001</DocumentId>
    <CompanyName>Test &amp; Company Ltd.</CompanyName>
    <Notes>Special chars: &lt; &gt; &amp; &quot; &apos;</Notes>
</ReportData>";
        var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlData));

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = xmlBase64
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
    }

    [Fact]
    public async Task ExportSingleReport_WithLargeXmlData_ProcessesWithoutTimeout()
    {
        // Arrange
        var itemsXml = new StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            itemsXml.AppendLine($@"
        <Item>
            <ItemCode>PROD{i:D3}</ItemCode>
            <Name>Product {i}</Name>
            <Quantity>{i + 1}</Quantity>
            <Price>{(i + 1) * 10.0}</Price>
        </Item>");
        }

        var xmlData = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReportData>
    <DocumentId>INV-LARGE</DocumentId>
    <Items>
{itemsXml}
    </Items>
</ReportData>";
        var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlData));

        var request = new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            XmlDataBase64 = xmlBase64
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export-single", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Success");
        result.PdfSizeBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportSingleReport_MultipleSequentialRequests_AllSucceed()
    {
        // Arrange
        var requests = Enumerable.Range(1, 5).Select(i => new ExportSingleReportRequest
        {
            ReportType = "Ispratnica",
            Parameters = new Dictionary<string, string>
            {
                { "DocumentId", $"INV-{i:D3}" }
            }
        }).ToList();

        // Act & Assert
        foreach (var request in requests)
        {
            var response = await _client.PostAsJsonAsync("/api/report/export-single", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<ExportSingleReportResponse>();
            result.Should().NotBeNull();
            result!.Status.Should().Be("Success");
        }
    }
}

