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
/// Integration tests specifically for DefaultFaktura report generation.
/// Tests the complete flow from report instantiation to PDF generation.
/// </summary>
public class DefaultFakturaIntegrationTests
{
    private readonly IReportFactory _reportFactory;
    private readonly ILogger<ReportService> _logger;

    public DefaultFakturaIntegrationTests()
    {
        _reportFactory = new ReportFactory();
        _logger = Mock.Of<ILogger<ReportService>>();
    }

    [Fact]
    public void DefaultFaktura_CanBeInstantiated()
    {
        // Act
        var report = new DefaultFaktura();

        // Assert
        report.Should().NotBeNull();
        report.Should().BeAssignableTo<XtraReport>();
    }

    [Fact]
    public void DefaultFaktura_HasRequiredBands()
    {
        // Arrange & Act
        var report = new DefaultFaktura();

        // Assert
        report.Bands.Count.Should().BeGreaterThan(0);
        report.Bands.GetBandByType(typeof(DetailBand)).Should().NotBeNull();
        report.Bands.GetBandByType(typeof(ReportHeaderBand)).Should().NotBeNull();
        report.Bands.GetBandByType(typeof(ReportFooterBand)).Should().NotBeNull();
    }

    [Fact]
    public void DefaultFaktura_HasRequiredParameters()
    {
        // Arrange & Act
        var report = new DefaultFaktura();

        // Assert
        report.Parameters.Count.Should().BeGreaterThan(0);
        report.Parameters["BrojNaFaktura"].Should().NotBeNull();
        report.Parameters["ImeNaFaktura"].Should().NotBeNull();
    }

    [Fact]
    public void DefaultFaktura_CanBindData()
    {
        // Arrange
        var report = new DefaultFaktura();
        var data = ReportDataHelper.CreateFakturaMainData("FAKT-001", DateTime.Now);

        // Act
        report.DataSource = data;
        var exception = Record.Exception(() => report.CreateDocument());

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void DefaultFaktura_GeneratesPdf()
    {
        // Arrange
        var report = new DefaultFaktura();
        var data = ReportDataHelper.CreateFakturaMainData("FAKT-002", DateTime.Now);
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
        pdfContent.Length.Should().BeGreaterThan(0);
        
        // Verify PDF header
        var header = System.Text.Encoding.ASCII.GetString(pdfContent.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public void DefaultFaktura_WithParameters_GeneratesPdf()
    {
        // Arrange
        var report = new DefaultFaktura();
        var data = ReportDataHelper.CreateFakturaMainData("FAKT-003", DateTime.Now);
        
        report.DataSource = data;
        report.Parameters["BrojNaFaktura"].Value = "FAKT-003";
        report.Parameters["ImeNaFaktura"].Value = "ФАКТУРА";
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
    public async Task ReportService_GeneratesFakturaPdf()
    {
        // Arrange
        var reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentType = "Faktura",
            DocumentIds = new List<string> { "FAKT001" }
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(1);
        
        var document = result.Documents.First();
        document.DocumentId.Should().Be("FAKT001");
        document.DocumentType.Should().Be("Faktura");
        document.PdfContent.Should().NotBeNull();
        document.PdfContent!.Length.Should().BeGreaterThan(0);
        
        // Verify it's a valid PDF
        var header = System.Text.Encoding.ASCII.GetString(document.PdfContent.Take(5).ToArray());
        header.Should().Be("%PDF-");
    }

    [Fact]
    public async Task ReportService_GeneratesMultipleFakturaPdfsInParallel()
    {
        // Arrange
        var reportService = new ReportService(_reportFactory, _logger);
        var documentIds = Enumerable.Range(1, 5).Select(i => $"FAKT{i:D3}").ToList();
        var filter = new ReportFilter
        {
            DocumentType = "Faktura",
            DocumentIds = documentIds
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Documents.Should().HaveCount(5);
        
        foreach (var document in result.Documents)
        {
            document.DocumentType.Should().Be("Faktura");
            document.PdfContent.Should().NotBeNull();
            document.PdfContent!.Length.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void DefaultFaktura_WithDynamicData_GeneratesPdf()
    {
        // Arrange
        var report = new DefaultFaktura();
        var data = ReportDataHelper.CreateFakturaData("FAKT-004", DateTime.Now);
        
        report.DataSource = data;
        report.Parameters["BrojNaFaktura"].Value = "FAKT-004";
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
    public void ReportFactory_CreatesFakturaReport()
    {
        // Act
        var report = _reportFactory.CreateReport("Faktura");

        // Assert
        report.Should().NotBeNull();
        report.Should().BeOfType<DefaultFaktura>();
    }

    [Fact]
    public void ReportFactory_CreatesInvoiceReport()
    {
        // Act
        var report = _reportFactory.CreateReport("Invoice");

        // Assert
        report.Should().NotBeNull();
        report.Should().BeOfType<DefaultFaktura>();
    }

    [Fact]
    public void ReportFactory_CreatesFakturaReportCaseInsensitive()
    {
        // Act
        var report1 = _reportFactory.CreateReport("faktura");
        var report2 = _reportFactory.CreateReport("FAKTURA");
        var report3 = _reportFactory.CreateReport("FaKtUrA");

        // Assert
        report1.Should().NotBeNull().And.BeOfType<DefaultFaktura>();
        report2.Should().NotBeNull().And.BeOfType<DefaultFaktura>();
        report3.Should().NotBeNull().And.BeOfType<DefaultFaktura>();
    }

    [Fact]
    public void ReportFactory_SupportsFakturaDocumentType()
    {
        // Act
        var supportedTypes = _reportFactory.GetSupportedDocumentTypes();

        // Assert
        supportedTypes.Should().Contain("Faktura");
        supportedTypes.Should().Contain("Invoice");
    }

    [Fact]
    public async Task ReportService_HandlesFakturaWithDifferentDateFormats()
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
                DocumentType = "Faktura",
                DocumentIds = new List<string> { $"FAKT_{date:yyyyMMdd}" }
            };

            var result = await reportService.ExportReportsToPdfAsync(filter);
            
            result.Should().NotBeNull();
            result.Documents.Should().HaveCount(1);
            result.Documents.First().PdfContent.Should().NotBeNull();
        }
    }

    [Fact]
    public void DefaultFaktura_ExportsToStream()
    {
        // Arrange
        var report = new DefaultFaktura();
        var data = ReportDataHelper.CreateFakturaMainData("FAKT-005", DateTime.Now);
        report.DataSource = data;
        report.CreateDocument();

        // Act
        using var stream = new MemoryStream();
        var exception = Record.Exception(() => report.ExportToPdf(stream));

        // Assert
        exception.Should().BeNull();
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DefaultFaktura_WithComplexData_GeneratesPdf()
    {
        // Arrange
        var report = new DefaultFaktura();
        var data = ReportDataHelper.CreateFakturaMainData("FAKT-006", DateTime.Now);
        
        report.DataSource = data;
        report.Parameters["BrojNaFaktura"].Value = "FAKT-006/2024";
        report.Parameters["ImeNaFaktura"].Value = "ФАКТУРА ЗА УСЛУГИ";
        report.Parameters["DatumNaValuta"].Value = DateTime.Now.AddDays(15);
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
    public void DefaultFaktura_HandlesEmptyDataGracefully()
    {
        // Arrange
        var report = new DefaultFaktura();
        var emptyData = new List<ReportDataHelper.FakturaItem>();
        report.DataSource = emptyData;

        // Act
        var exception = Record.Exception(() => report.CreateDocument());

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task ReportService_GeneratesFakturaWithCorrectMetadata()
    {
        // Arrange
        var reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentType = "Faktura",
            DocumentIds = new List<string> { "FAKT-META-001" }
        };

        // Act
        var result = await reportService.ExportReportsToPdfAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.ExportDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        var document = result.Documents.First();
        document.DocumentId.Should().Be("FAKT-META-001");
        document.DocumentType.Should().Be("Faktura");
        document.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DefaultFaktura_DataBindings_AreCorrect()
    {
        // Arrange
        var report = new DefaultFaktura();

        // Act
        var detailBand = report.Bands.GetBandByType(typeof(DetailBand)) as DetailBand;

        // Assert
        detailBand.Should().NotBeNull();
        detailBand!.Controls.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ReportService_HandlesCancellationForFaktura()
    {
        // Arrange
        var reportService = new ReportService(_reportFactory, _logger);
        var filter = new ReportFilter
        {
            DocumentType = "Faktura",
            DocumentIds = Enumerable.Range(1, 100).Select(i => $"FAKT{i:D4}").ToList()
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await reportService.ExportReportsToPdfAsync(filter, cts.Token);
        });
    }

    [Fact]
    public void DefaultFaktura_SupportsMultipleItems()
    {
        // Arrange
        var report = new DefaultFaktura();
        var data = ReportDataHelper.CreateFakturaMainData("FAKT-MULTI", DateTime.Now);

        // Act
        report.DataSource = data;
        report.CreateDocument();

        // Assert
        data.Should().HaveCountGreaterThan(1);
        report.Pages.Should().NotBeNull();
    }
}

