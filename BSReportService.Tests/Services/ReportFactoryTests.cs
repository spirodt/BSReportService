using BSReportService.BSReports.MFakturi;
using BSReportService.Services;
using FluentAssertions;
using Xunit;

namespace BSReportService.Tests.Services;

/// <summary>
/// Unit tests for ReportFactory to verify report instantiation and mapping.
/// </summary>
public class ReportFactoryTests
{
    private readonly IReportFactory _factory;

    public ReportFactoryTests()
    {
        _factory = new ReportFactory();
    }

    [Fact]
    public void CreateReport_WithIspratnica_ReturnsReport()
    {
        // Act
        var report = _factory.CreateReport("Ispratnica");

        // Assert
        report.Should().NotBeNull();
        report.Should().BeOfType<frmIspratnica>();
    }

    [Fact]
    public void CreateReport_WithInvoice_ReturnsReport()
    {
        // Act
        var report = _factory.CreateReport("Invoice");

        // Assert
        report.Should().NotBeNull();
        report.Should().BeOfType<DefaultFaktura>();
    }

    [Fact]
    public void CreateReport_CaseInsensitive_ReturnsReport()
    {
        // Act
        var report1 = _factory.CreateReport("ispratnica");
        var report2 = _factory.CreateReport("ISPRATNICA");
        var report3 = _factory.CreateReport("IsPrAtNiCa");

        // Assert
        report1.Should().NotBeNull();
        report2.Should().NotBeNull();
        report3.Should().NotBeNull();
    }

    [Fact]
    public void CreateReport_WithUnknownType_ReturnsNull()
    {
        // Act
        var report = _factory.CreateReport("UnknownType");

        // Assert
        report.Should().BeNull();
    }

    [Fact]
    public void CreateReport_WithNullType_ReturnsNull()
    {
        // Act
        var report = _factory.CreateReport(null!);

        // Assert
        report.Should().BeNull();
    }

    [Fact]
    public void CreateReport_WithEmptyType_ReturnsNull()
    {
        // Act
        var report = _factory.CreateReport("");

        // Assert
        report.Should().BeNull();
    }

    [Fact]
    public void GetSupportedDocumentTypes_ReturnsAllTypes()
    {
        // Act
        var types = _factory.GetSupportedDocumentTypes();

        // Assert
        types.Should().NotBeEmpty();
        types.Should().Contain("Ispratnica");
        types.Should().Contain("Invoice");
    }

    [Fact]
    public void GetSupportedDocumentTypes_ReturnsSameKeysAsMappings()
    {
        // Act
        var types = _factory.GetSupportedDocumentTypes().ToList();

        // Assert - All returned types should create valid reports
        foreach (var type in types)
        {
            var report = _factory.CreateReport(type);
            report.Should().NotBeNull($"Type '{type}' should create a valid report");
        }
    }

    [Fact]
    public void CreateReport_MultipleInstances_AreIndependent()
    {
        // Act
        var report1 = _factory.CreateReport("Ispratnica");
        var report2 = _factory.CreateReport("Ispratnica");

        // Assert
        report1.Should().NotBeSameAs(report2, "Each call should create a new instance");
    }

    [Theory]
    [InlineData("Ispratnica")]
    [InlineData("Invoice")]
    public void CreateReport_ValidTypes_CreatesNewInstanceEachTime(string documentType)
    {
        // Act
        var report1 = _factory.CreateReport(documentType);
        var report2 = _factory.CreateReport(documentType);
        var report3 = _factory.CreateReport(documentType);

        // Assert
        report1.Should().NotBeNull();
        report2.Should().NotBeNull();
        report3.Should().NotBeNull();
        
        report1.Should().NotBeSameAs(report2);
        report1.Should().NotBeSameAs(report3);
        report2.Should().NotBeSameAs(report3);
    }
}

