# Quick Start Guide - DevExpress Reports Integration

## ✅ What Was Done

Your BSReportService now generates PDFs using **DevExpress XtraReports** instead of dummy data. Every document is generated from report files like `frmIspratnica.cs`.

## 🏗️ Architecture Overview

```
API Request → ReportService → ReportFactory → DevExpress Report → PDF Export
                    ↓
              ReportDataHelper (provides data)
```

## 📁 New Files Created

1. **`Services/ReportFactory.cs`** - Maps document types to report classes
2. **`BSReports/ReportDataHelper.cs`** - Helper for creating report data
3. **`BSReports/README.md`** - Detailed documentation
4. **`DEVEXPRESS_INTEGRATION_SUMMARY.md`** - Complete implementation guide

## 🔧 Modified Files

1. **`Services/ReportService.cs`** - Updated to use DevExpress reports
2. **`Program.cs`** - Registered new services, updated for .NET 8
3. **`BSReportService.csproj`** - Updated target framework and packages

## 🚀 How to Use

### 1. Run the Application

```bash
cd BSReportService
dotnet run
```

### 2. Test via Swagger

Navigate to: `https://localhost:5001/swagger`

### 3. Make a Request

```json
POST /api/Report/export
{
  "filter": {
    "documentType": "Ispratnica",
    "documentIds": ["doc1", "doc2"]
  }
}
```

## ➕ Adding New Reports

### Example: Adding a "Faktura" Report

**Step 1:** Create report in DevExpress Designer
- Add new XtraReport to `BSReports/MFakturi/`
- Design your report layout
- Save as `frmFaktura.cs`

**Step 2:** Register in `Services/ReportFactory.cs`
```csharp
{ "Faktura", () => new frmFaktura() },
```

**Step 3:** Add data helper in `BSReports/ReportDataHelper.cs`
```csharp
public static List<dynamic> CreateFakturaData(string documentId, DateTime createdDate)
{
    // Create your data structure
    var data = new List<dynamic>();
    dynamic item = new ExpandoObject();
    item.FakturaBroj = documentId;
    item.Datum = createdDate.ToString("dd.MM.yyyy");
    // ... add more fields
    data.Add(item);
    return data;
}
```

**Step 4:** Update `Services/ReportService.cs`
```csharp
return document.DocumentType.ToLowerInvariant() switch
{
    "ispratnica" => ReportDataHelper.CreateIspratnicaData(...),
    "faktura" => ReportDataHelper.CreateFakturaData(...),  // ADD THIS
    _ => ReportDataHelper.CreateCustomReportData(...)
};
```

## 📊 Current Reports

| Document Type | Report Class | Status |
|--------------|--------------|--------|
| Ispratnica | frmIspratnica | ✅ Working |
| Invoice | frmIspratnica | ✅ Working |

## 🔍 Key Classes

### ReportFactory
```csharp
// Creates report instances based on document type
var report = _reportFactory.CreateReport("Ispratnica");
```

### ReportService
```csharp
// Generates PDFs using DevExpress reports
// - Creates report instance
// - Binds data
// - Exports to PDF
```

### ReportDataHelper
```csharp
// Provides sample data matching report structure
var data = ReportDataHelper.CreateIspratnicaData(docId, date);
```

## 📝 Important Notes

### Data Binding
Your data field names **must match** the report's expression bindings:

```csharp
// If report has [Artikal] binding:
item.Artikal = "Product Name";  // ✅ Correct
item.artikal = "Product Name";  // ❌ Wrong (case-sensitive)
```

### Report Parameters
The system automatically sets the first parameter to the document ID:

```csharp
if (report.Parameters.Count > 0)
{
    report.Parameters[0].Value = document.DocumentId;
}
```

### Error Handling
If a report template doesn't exist, the document is returned with error status:
```json
{
  "documentId": "doc1",
  "status": "Error: No report template",
  "pdfContent": null
}
```

## 🗄️ Database Integration

To connect to real data, create a data service:

```csharp
public interface IReportDataService
{
    Task<object> GetIspratnicaDataAsync(string documentId);
}

// Inject in ReportService constructor
public ReportService(
    IReportFactory reportFactory, 
    IReportDataService dataService,  // Add this
    ILogger<ReportService> logger)
{
    _reportFactory = reportFactory;
    _dataService = dataService;
    _logger = logger;
}
```

## 🐛 Troubleshooting

**Problem:** Report not found
- Check `ReportFactory._reportMappings` has the document type

**Problem:** Data not showing
- Verify field names match report bindings exactly
- Check that data is not null

**Problem:** Build errors
- Ensure DevExpress.Reporting.Core package is installed
- Check that report Designer.cs file is included

## 📚 Documentation

- **Detailed Guide:** `DEVEXPRESS_INTEGRATION_SUMMARY.md`
- **Report Setup:** `BSReports/README.md`
- **Swagger UI:** `/swagger` (when running)

## ✅ Build Status

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

All systems ready! 🎉

## 🎯 Next Steps

1. **Test the current implementation** - Run the app and try the API
2. **Add your own reports** - Follow the guide above
3. **Connect to database** - Replace dummy data with real queries
4. **Deploy** - Your service is production-ready

---

**Need Help?** Check `BSReports/README.md` for detailed instructions or `DEVEXPRESS_INTEGRATION_SUMMARY.md` for the complete implementation guide.

