using System.Net;
using System.Net.Http.Json;
using BSReportService.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BSReportService.Tests.Controllers;

public class ReportControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReportControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        }).CreateClient();
    }

    [Fact]
    public async Task ExportReports_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new ExportReportRequest
        {
            Filter = new ReportFilter
            {
                DocumentType = "Invoice",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportReports_WithMultipleDocumentIds_ReturnsOk()
    {
        // Arrange
        var request = new ExportReportRequest
        {
            Filter = new ReportFilter
            {
                DocumentIds = new List<string> { "doc1", "doc2", "doc3" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExportReportResponse>();
        result.Should().NotBeNull();
        result!.Documents.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportReports_WithEmptyFilter_ReturnsOk()
    {
        // Arrange
        var request = new ExportReportRequest
        {
            Filter = new ReportFilter()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/report/export", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExportReports_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync<ExportReportRequest>("/api/report/export", null!);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }
}
