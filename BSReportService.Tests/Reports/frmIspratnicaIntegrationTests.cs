using BSReportService.BSReports;
using BSReportService.BSReports.MFakturi;
using BSReportService.Models;
using BSReportService.Services;
using DevExpress.XtraReports.UI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BSReportService.Tests.Reports;

/// <summary>
/// Integration tests specifically for frmIspratnica report generation.
/// Tests the complete flow from report instantiation to PDF generation.
/// </summary>
public class frmIspratnicaIntegrationTests
{
    private readonly IReportFactory _reportFactory;
    private readonly ILogger<ReportService> _logger;

    public frmIspratnicaIntegrationTests()
    {
        _reportFactory = new ReportFactory();
        _logger = Mock.Of<ILogger<ReportService>>();
    }

    [Fact]
    public void frmIspratnica_CanBeInstantiated()
    {
        // Act
        var report = new frmIspratnica();

        // Assert
        report.Should().NotBeNull();
        report.Should().BeAssignableTo<XtraReport>();
    }

    [Fact]
    public void frmIspratnica_HasRequiredBands()
    {
        // Arrange & Act
        var report = new frmIspratnica();

        // Assert
        report.Bands.Count.Should().BeGreaterThan(0);
        report.Bands.GetBandByType(typeof(DetailBand)).Should().NotBeNull();
    }

    [Fact]
    public void frmIspratnica_HasFakturaParameter()
    {
        // Arrange & Act
        var report = new frmIspratnica();

        // Assert
        report.Parameters.Count.Should().BeGreaterThan(0);
        report.Parameters["Faktura"].Should().NotBeNull();
    }

    [Fact]
    public void frmIspratnica_CanBindData()
    {
        // Arrange
        var report = new frmIspratnica();
        var data = ReportDataHelper.CreateIspratnicaData("TEST001", DateTime.Now);

        // Act
        report.DataSource = data;
        var exception = Record.Exception(() => report.CreateDocument());

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void frmIspratnica_GeneratesPdfContent()
    {
        // Arrange
        var report = new frmIspratnica();
        var data = ReportDataHelper.CreateIspratnicaData("TEST001", DateTime.Now);
        report.DataSource = data;
        report.CreateDocument();

        // Act
        byte[] pdfContent;
        using (var memoryStream = new MemoryStream())
        {
            report.ExportToPdf(memoryStream);
            pdfContent = memoryStream.ToArray();
        }

        // Assert
        pdfContent.Should().NotBeNull();
        pdfContent.Length.Should().BeGreaterThan(0, "PDF should have content");
        
        // Verify PDF header (PDF files start with %PDF-)
        var header = System.Text.Encoding.ASCII.GetString(pdfContent.Take(5).ToArray());
        header.Should().Be("%PDF-", "Generated content should be a valid PDF");
    }

    [Fact]
    public void frmIspratnica_GeneratesPdfWithMultipleItems()
    {
        // Arrange
        var report = new frmIspratnica();
        var data = ReportDataHelper.CreateIspratnicaData("TEST002", DateTime.Now);
        report.DataSource = data;
        report.CreateDocument();

        // Act
        byte[] pdfContent;
        using (var memoryStream = new MemoryStream())
        {
            report.ExportToPdf(memoryStream);
            pdfContent = memoryStream.ToArray();
        }

        // Assert
        pdfContent.Should().NotBeNull();
        pdfContent.Length.Should().BeGreaterThan(1000, "PDF with multiple items should be substantial");
    }

    [Fact]
    public void frmIspratnica_WithParameters_GeneratesPdf()
    {
        // Arrange
        var report = new frmIspratnica();
        var data = ReportDataHelper.CreateIspratnicaData("TEST003", DateTime.Now);
        
        report.DataSource = data;
        report.Parameters["Faktura"].Value = "ISPRATNICA-TEST003";
        report.CreateDocument();

        // Act
        byte[] pdfContent;
        using (var memoryStream = new MemoryStream())
        {
            report.ExportToPdf(memoryStream);
            pdfContent = memoryStream.ToArray();
        }

        // Assert
        pdfContent.Should().NotBeNull();
        pdfContent.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ReportService_GeneratesIspratnicaPdf()
    {
        // Arrange
        var reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentType = "Ispratnica",
            DocumentIds = new List<string> { "ISP001" }
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(1);
        
        var document = result.Documents.First();
        document.DocumentId.Should().Be("ISP001");
        document.DocumentType.Should().Be("Ispratnica");
        document.PdfContent.Should().NotBeNull();
        document.PdfContent!.Length.Should().BeGreaterThan(0);
        
        // Verify it's a valid PDF
        var header = System.Text.Encoding.ASCII.GetString(document.PdfContent.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task ReportService_GeneratesMultipleIspratnicaPdfsInParallel()
    {
        // Arrange
        var reportService = new ReportService(_reportFactory, _logger);
        var documentIds = Enumerable.Range(1, 10).Select(i => $"ISP{i:000}").ToList();
        
        var filter = new ReportFilter
        {
            DocumentType = "Ispratnica",
            DocumentIds = documentIds
        };

        // Act
        var startTime = DateTime.UtcNow;
        var result = await reportService.ExportReportsToPdfAsync(filter);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(10);
        
        result.Documents.Should().AllSatisfy(doc =>
        {
            doc.DocumentType.Should().Be("Ispratnica");
            doc.PdfContent.Should().NotBeNull();
            doc.PdfContent!.Length.Should().BeGreaterThan(0);
        });

        // Verify parallel processing is reasonably fast
        elapsed.TotalSeconds.Should().BeLessThan(30, "Parallel processing should be efficient");
    }

    [Fact]
    public void ReportDataHelper_CreatesValidIspratnicaData()
    {
        // Act
        var data = ReportDataHelper.CreateIspratnicaData("TEST001", DateTime.Now);

        // Assert
        data.Should().NotBeNull();
        data.Should().NotBeEmpty();
        data.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ReportFactory_CreatesIspratnicaReport()
    {
        // Act
        var report = _reportFactory.CreateReport("Ispratnica");

        // Assert
        report.Should().NotBeNull();
        report.Should().BeOfType<frmIspratnica>();
    }

    [Fact]
    public void ReportFactory_CreatesIspratnicaReportCaseInsensitive()
    {
        // Act
        var report1 = _reportFactory.CreateReport("ispratnica");
        var report2 = _reportFactory.CreateReport("ISPRATNICA");
        var report3 = _reportFactory.CreateReport("IsPrAtNiCa");

        // Assert
        report1.Should().NotBeNull().And.BeOfType<frmIspratnica>();
        report2.Should().NotBeNull().And.BeOfType<frmIspratnica>();
        report3.Should().NotBeNull().And.BeOfType<frmIspratnica>();
    }

    [Fact]
    public void ReportFactory_SupportsIspratnicaDocumentType()
    {
        // Act
        var supportedTypes = _reportFactory.GetSupportedDocumentTypes();

        // Assert
        supportedTypes.Should().Contain("Ispratnica");
    }

    [Fact]
    public async Task ReportService_HandlesIspratnicaWithDifferentDateFormats()
    {
        // Arrange
        var reportService = new ReportService(_reportFactory, _logger);
        var dates = new[]
        {
            DateTime.Now,
            DateTime.Now.AddDays(-30),
            DateTime.Now.AddMonths(-6),
            DateTime.Now.AddYears(-1)
        };

        // Act & Assert
        foreach (var date in dates)
        {
            var filter = new ReportFilter
            {
                DocumentType = "Ispratnica",
                DocumentIds = new List<string> { $"ISP_{date:yyyyMMdd}" }
            };

            var result = await reportService.ExportReportsToPdfAsync(filter);
            
            result.Should().NotBeNull();
            result.Documents.Should().HaveCount(1);
            result.Documents.First().PdfContent.Should().NotBeNull();
        }
    }

    [Fact]
    public void frmIspratnica_ExportsToStream()
    {
        // Arrange
        var report = new frmIspratnica();
        var data = ReportDataHelper.CreateIspratnicaData("TEST004", DateTime.Now);
        report.DataSource = data;
        report.CreateDocument();

        // Act
        using var stream = new MemoryStream();
        var exception = Record.Exception(() => report.ExportToPdf(stream));

        // Assert
        exception.Should().BeNull("Export to stream should succeed");
        stream.Length.Should().BeGreaterThan(0, "Stream should contain PDF data");
        stream.Position = 0;
        
        var buffer = new byte[5];
        stream.Read(buffer, 0, 5);
        var header = System.Text.Encoding.ASCII.GetString(buffer);
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task EndToEnd_IspratnicaReportGeneration()
    {
        // This test simulates the complete end-to-end flow
        
        // Arrange - Create the service stack
        var factory = new ReportFactory();
        var logger = Mock.Of<ILogger<ReportService>>();
        var service = new ReportService(factory, logger);

        var filter = new ReportFilter
        {
            DocumentType = "Ispratnica",
            DocumentIds = new List<string> { "E2E-001" }
            // Note: Not using date filters as they might filter out our test document
        };

        // Act - Execute the service
        var response = await service.ExportReportsToPdfAsync(filter);

        // Assert - Verify complete response
        response.Should().NotBeNull();
        response.TotalCount.Should().Be(1);
        response.ExportDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        var document = response.Documents.First();
        document.DocumentId.Should().Be("E2E-001");
        document.DocumentType.Should().Be("Ispratnica");
        document.Status.Should().NotBeNullOrEmpty();
        document.CreatedDate.Should().BeOnOrBefore(DateTime.UtcNow);
        document.PdfContent.Should().NotBeNull();
        document.PdfContent!.Length.Should().BeGreaterThan(1000);
        
        // Verify PDF structure
        var pdfHeader = System.Text.Encoding.ASCII.GetString(document.PdfContent.Take(5).ToArray());
        pdfHeader.Should().Be("%PDF-", "Generated content should be a valid PDF file");
    }
}

