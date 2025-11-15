# BS Report Service

A .NET 9 Web API service for exporting multiple documents to PDF simultaneously.

## Features

- **Parallel PDF Export**: Processes multiple documents to PDF in parallel for improved performance
- **Filtering Support**: Filter documents by type, date range, status, or specific document IDs
- **RESTful API**: Clean REST API endpoint for exporting reports
- **Comprehensive Testing**: Unit tests and integration tests included

## Project Structure

```
BSReportService/
├── BSReportService/              # Main API project
│   ├── Controllers/
│   │   └── ReportController.cs   # API controller for report export
│   ├── Models/                   # Data models and DTOs
│   │   ├── ExportReportRequest.cs
│   │   ├── ExportReportResponse.cs
│   │   ├── ReportDocument.cs
│   │   └── ReportFilter.cs
│   ├── Services/                 # Business logic services
│   │   ├── IReportService.cs
│   │   └── ReportService.cs      # Implementation with dummy PDF export
│   └── Program.cs                # Application entry point
└── BSReportService.Tests/        # Test project
    ├── Controllers/
    │   └── ReportControllerTests.cs
    └── Services/
        ├── ReportServiceTests.cs
        └── ReportServiceIntegrationTests.cs
```

## API Endpoint

### POST /api/report/export

Exports multiple documents to PDF based on filter criteria.

**Request Body:**
```json
{
  "filter": {
    "documentType": "Invoice",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-12-31T23:59:59Z",
    "status": "Active",
    "documentIds": ["doc1", "doc2", "doc3"]
  }
}
```

**Response:**
```json
{
  "documents": [
    {
      "documentId": "doc1",
      "documentType": "Invoice",
      "status": "Active",
      "createdDate": "2024-01-15T10:00:00Z",
      "pdfContent": "..."
    }
  ],
  "totalCount": 1,
  "exportDate": "2024-12-14T12:00:00Z"
}
```

## Filter Parameters

The `ReportFilter` supports the following optional parameters:

- `DocumentType`: Filter by document type (e.g., "Invoice", "Report")
- `StartDate`: Filter documents created on or after this date
- `EndDate`: Filter documents created on or before this date
- `Status`: Filter by document status
- `DocumentIds`: List of specific document IDs to export

## Implementation Notes

### Current Implementation

The current implementation includes **dummy PDF export logic**. The `ReportService`:

1. Filters documents based on the provided criteria
2. Processes documents in parallel using `Task.WhenAll`
3. Generates dummy PDF content (returns UTF-8 encoded strings)

### Next Steps

To implement actual PDF generation, replace the `GenerateDummyPdfContent` method in `ReportService.cs` with a real PDF library such as:

- **iTextSharp** / **iText7**: Popular PDF generation library
- **PdfSharp**: Another option for PDF creation
- **QuestPDF**: Modern PDF generation library for .NET

Example integration point:
```csharp
private byte[] GenerateDummyPdfContent(ReportDocument document)
{
    // TODO: Replace with actual PDF generation
    // Example: Use iTextSharp, PdfSharp, or QuestPDF to generate PDF
    var content = $"PDF content for document {document.DocumentId}";
    return System.Text.Encoding.UTF8.GetBytes(content);
}
```

## Running the Application

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the API
dotnet run --project BSReportService
```

The API will be available at `https://localhost:5001` (or the configured port).

## Testing

The project includes comprehensive tests:

- **Unit Tests**: Test service behavior with mocked dependencies
- **Integration Tests**: Test the full API endpoints using `WebApplicationFactory`

Run tests:
```bash
dotnet test
```

## Requirements

- .NET 9 SDK or later
- Windows, Linux, or macOS

## License

This project is a skeleton/template for a report export service.
