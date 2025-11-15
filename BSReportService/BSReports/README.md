# DevExpress Report Integration Guide

This document explains how to integrate DevExpress XtraReport files into the BSReportService system.

## Overview

The BSReportService uses a factory pattern to dynamically instantiate and render DevExpress reports based on document types. Each report template is a class that inherits from `DevExpress.XtraReports.UI.XtraReport`.

## Report File Structure

Reports are organized in subdirectories based on their category:

```
BSReports/
├── MFakturi/               # Invoice-related reports
│   ├── frmIspratnica.cs    # Report class
│   ├── frmIspratnica.Designer.cs  # Designer-generated code
│   └── frmIspratnica.resx  # Report resources
├── ReportDataHelper.cs     # Helper for creating report data
└── README.md              # This file
```

## How Reports Work

### 1. Report Files

Each DevExpress report consists of three files:
- **`frmReportName.cs`** - The main report class
- **`frmReportName.Designer.cs`** - Designer-generated code (contains report layout and controls)
- **`frmReportName.resx`** - Resources (images, strings, etc.)

### 2. Report Class Structure

A typical report class looks like this:

```csharp
using DevExpress.XtraReports.UI;

namespace BSReportService.BSReports.MFakturi
{
    public partial class frmIspratnica : DevExpress.XtraReports.UI.XtraReport
    {
        public frmIspratnica()
        {
            InitializeComponent();
        }
    }
}
```

### 3. Report Registration

Reports must be registered in the `ReportFactory` to be available for use.

## Adding a New Report

Follow these steps to add a new report to the system:

### Step 1: Create the Report Using DevExpress Report Designer

1. Open your project in Visual Studio
2. Add a new DevExpress Report item to the appropriate folder
3. Design your report using the DevExpress Report Designer
4. Add data bindings, bands, and controls as needed
5. Save the report

### Step 2: Register the Report in ReportFactory

Open `Services/ReportFactory.cs` and add your report to the `_reportMappings` dictionary:

```csharp
_reportMappings = new Dictionary<string, Func<XtraReport>>(StringComparer.OrdinalIgnoreCase)
{
    { "Ispratnica", () => new frmIspratnica() },
    { "Invoice", () => new frmIspratnica() },
    
    // Add your new report here:
    { "Faktura", () => new frmFaktura() },
    { "Proforma", () => new frmProforma() },
};
```

The key is the document type that will be used in API requests.

### Step 3: Create Data Helper Method

Add a method in `BSReports/ReportDataHelper.cs` to generate sample data for your report:

```csharp
public static List<dynamic> CreateFakturaData(string documentId, DateTime createdDate)
{
    var data = new List<dynamic>();
    
    // Create data structure that matches your report's data bindings
    dynamic item = new ExpandoObject();
    item.FakturaNumber = documentId;
    item.Date = createdDate;
    item.CustomerName = "Sample Customer";
    // ... add more fields as needed
    
    data.Add(item);
    return data;
}
```

### Step 4: Update GetReportDataAsync in ReportService

Add your report type to the switch statement in `Services/ReportService.cs`:

```csharp
private async Task<object> GetReportDataAsync(ReportDocument document, CancellationToken cancellationToken)
{
    await Task.Delay(10, cancellationToken);
    
    return document.DocumentType.ToLowerInvariant() switch
    {
        "ispratnica" => ReportDataHelper.CreateIspratnicaData(document.DocumentId, document.CreatedDate),
        "faktura" => ReportDataHelper.CreateFakturaData(document.DocumentId, document.CreatedDate),
        _ => ReportDataHelper.CreateCustomReportData(document.DocumentId, document.CreatedDate)
    };
}
```

### Step 5: Test Your Report

Use the API to test your report:

```json
POST /api/Report/export
{
  "filter": {
    "documentType": "Faktura",
    "documentIds": ["doc1"]
  }
}
```

## Report Data Binding

### Understanding Data Bindings

DevExpress reports use expression bindings to connect controls to data fields. In the Designer.cs file, you'll see code like:

```csharp
this.Artikal1.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
    new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Artikal]")
});
```

This means the control `Artikal1` will display the value from the `Artikal` field in your data source.

### Creating Compatible Data

Your data structure must have properties that match the field names used in expressions:

```csharp
// Report expects [Artikal] field
dynamic item = new ExpandoObject();
item.Artikal = "Product Name";  // This matches the binding
```

### Setting the Data Source

The `ReportService` automatically sets the data source:

```csharp
report.DataSource = reportData;
report.CreateDocument();  // Binds data to controls
```

## Report Parameters

If your report uses parameters (like the `Faktura` parameter in `frmIspratnica`):

### In Report Designer

Parameters are defined in the Designer:

```csharp
this.Faktura = new DevExpress.XtraReports.Parameters.Parameter();
this.Faktura.Description = "Enter Faktura:";
this.Faktura.Name = "Faktura";
```

### Setting Parameters

The `ReportService` automatically sets the first parameter to the document ID. For custom parameter handling, modify this code in `ReportService.cs`:

```csharp
if (report.Parameters.Count > 0)
{
    // Custom parameter mapping
    if (report.Parameters["Faktura"] != null)
    {
        report.Parameters["Faktura"].Value = document.DocumentId;
    }
}
```

## Best Practices

### 1. Naming Conventions

- Use `frm` prefix for report class names (e.g., `frmIspratnica`)
- Use descriptive document type keys (e.g., "Ispratnica", not "Report1")

### 2. Data Structure

- Use `ExpandoObject` for dynamic data structures
- Match field names exactly with report bindings (case-sensitive)
- Include all fields required by the report to avoid binding errors

### 3. Error Handling

The system handles errors gracefully:
- If no report template exists for a document type, the document is marked with error status
- If PDF generation fails, the error is logged and returned in the response

### 4. Performance

- Reports are generated in parallel for multiple documents
- Use `Task.Run` to execute potentially long-running report operations
- Consider caching frequently used reports or data

## Database Integration

To connect reports to real database data:

### Step 1: Create a Data Access Service

```csharp
public interface IReportDataService
{
    Task<object> GetIspratnicaDataAsync(string documentId);
    Task<object> GetFakturaDataAsync(string documentId);
}
```

### Step 2: Update ReportService

Inject the data service and use it in `GetReportDataAsync`:

```csharp
private readonly IReportDataService _dataService;

public ReportService(IReportFactory reportFactory, IReportDataService dataService, ILogger<ReportService> logger)
{
    _reportFactory = reportFactory;
    _dataService = dataService;
    _logger = logger;
}

private async Task<object> GetReportDataAsync(ReportDocument document, CancellationToken cancellationToken)
{
    return document.DocumentType.ToLowerInvariant() switch
    {
        "ispratnica" => await _dataService.GetIspratnicaDataAsync(document.DocumentId),
        "faktura" => await _dataService.GetFakturaDataAsync(document.DocumentId),
        _ => ReportDataHelper.CreateCustomReportData(document.DocumentId, document.CreatedDate)
    };
}
```

## Troubleshooting

### Report Not Generating

1. **Check report registration** - Is the document type registered in `ReportFactory`?
2. **Check data bindings** - Do the field names in data match the expressions in the report?
3. **Check parameters** - Are all required parameters being set?

### Data Not Displaying

1. **Verify data source** - Is the data being set correctly on the report?
2. **Check expressions** - Are the expression bindings correct in the Designer?
3. **Verify field names** - Field names are case-sensitive

### PDF Export Fails

1. **Check DevExpress license** - Ensure DevExpress.Reporting.Core is properly licensed
2. **Check memory** - Large reports may require more memory
3. **Check logs** - Review the error logs for specific error messages

## Example: Complete New Report Integration

Here's a complete example of adding a new "Faktura" report:

### 1. ReportFactory.cs

```csharp
{ "Faktura", () => new frmFaktura() },
```

### 2. ReportDataHelper.cs

```csharp
public static List<dynamic> CreateFakturaData(string documentId, DateTime createdDate)
{
    var data = new List<dynamic>();
    
    for (int i = 1; i <= 5; i++)
    {
        dynamic item = new ExpandoObject();
        item.RedniBroj = i;
        item.Artikal = $"Product {i}";
        item.Kolicina = i * 10;
        item.Cena = 100.00m * i;
        item.Iznos = item.Kolicina * item.Cena;
        item.FakturaBroj = documentId;
        item.Datum = createdDate.ToString("dd.MM.yyyy");
        item.Kupuvac = "Sample Customer Ltd.";
        data.Add(item);
    }
    
    return data;
}
```

### 3. ReportService.cs

```csharp
return document.DocumentType.ToLowerInvariant() switch
{
    "ispratnica" => ReportDataHelper.CreateIspratnicaData(document.DocumentId, document.CreatedDate),
    "faktura" => ReportDataHelper.CreateFakturaData(document.DocumentId, document.CreatedDate),
    _ => ReportDataHelper.CreateCustomReportData(document.DocumentId, document.CreatedDate)
};
```

## Additional Resources

- [DevExpress Reporting Documentation](https://docs.devexpress.com/XtraReports/2162/reporting)
- [Creating Reports from Scratch](https://docs.devexpress.com/XtraReports/4231/detailed-guide-to-devexpress-reporting/create-reports)
- [Data Binding](https://docs.devexpress.com/XtraReports/2997/detailed-guide-to-devexpress-reporting/data-binding)

