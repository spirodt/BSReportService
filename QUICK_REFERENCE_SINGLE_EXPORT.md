# Quick Reference: Single Report Export

## API Endpoint

```
POST https://localhost:7218/api/report/export-single
Content-Type: application/json
```

## Request Model

```json
{
  "reportType": "string (required)",
  "outputPath": "string (optional)",
  "xmlDataBase64": "string (optional)",
  "parameters": {
    "key": "value"
  }
}
```

## Response Model

```json
{
  "reportType": "string",
  "pdfContent": [byte array],
  "pdfContentBase64": "string",
  "savedFilePath": "string",
  "pdfSizeBytes": number,
  "generatedDate": "datetime",
  "status": "Success|Error",
  "errorMessage": "string"
}
```

## VBA Quick Start

### Import Module
1. Download `ReportServiceAPI.bas`
2. In VBA Editor: File > Import File
3. Select `ReportServiceAPI.bas`

### Simple Export
```vba
Sub ExportReport()
    Dim success As Boolean
    success = ExportReportSimple("Ispratnica", _
                                  "C:\Reports\invoice.pdf", _
                                  "INV-001", _
                                  "Test Company")
    If success Then MsgBox "Done!"
End Sub
```

### Export with XML
```vba
Sub ExportWithXML()
    Dim xml As String
    xml = "<?xml version='1.0'?>" & _
          "<ReportData>" & _
          "  <DocumentId>INV-001</DocumentId>" & _
          "</ReportData>"
    
    Dim success As Boolean
    success = ExportReportWithXml("Ispratnica", _
                                   "C:\Reports\invoice.pdf", _
                                   xml)
    If success Then MsgBox "Done!"
End Sub
```

## C# Quick Start

```csharp
var request = new ExportSingleReportRequest
{
    ReportType = "Ispratnica",
    OutputPath = @"C:\Reports\invoice.pdf",
    Parameters = new Dictionary<string, string>
    {
        ["DocumentId"] = "INV-001"
    }
};

var response = await client.PostAsJsonAsync(
    "/api/report/export-single", request);
var result = await response.Content
    .ReadFromJsonAsync<ExportSingleReportResponse>();
```

## PowerShell Quick Start

```powershell
$body = @{
    reportType = "Ispratnica"
    outputPath = "C:\Reports\invoice.pdf"
    parameters = @{
        DocumentId = "INV-001"
    }
} | ConvertTo-Json

Invoke-RestMethod `
    -Uri "https://localhost:7218/api/report/export-single" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

## XML Structure Template

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ReportData>
  <Header>
    <DocumentId>INV-001</DocumentId>
    <CompanyName>Company Name</CompanyName>
    <Date>2024-01-15</Date>
  </Header>
  <Items>
    <Item>
      <Name>Product Name</Name>
      <Quantity>10</Quantity>
      <Price>100.00</Price>
    </Item>
  </Items>
</ReportData>
```

## Base64 Encoding

### PowerShell
```powershell
[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($xml))
```

### C#
```csharp
Convert.ToBase64String(Encoding.UTF8.GetBytes(xml))
```

### VBA
```vba
Function EncodeBase64(text As String) As String
    Dim objXML As Object
    Dim objNode As Object
    Set objXML = CreateObject("MSXML2.DOMDocument")
    Set objNode = objXML.createElement("b64")
    objNode.DataType = "bin.base64"
    objNode.nodeTypedValue = StrConv(text, vbFromUnicode)
    EncodeBase64 = objNode.text
End Function
```

## Common Use Cases

### 1. Export from Excel Data
```vba
' In Excel VBA
Sub ExportCurrentSheet()
    Dim xml As String
    xml = BuildXmlFromRange(Range("A1:D10"), "B2", "B3", 7)
    Call ExportReportWithXml("Ispratnica", _
                             "C:\Reports\current.pdf", xml)
End Sub
```

### 2. Batch Export Multiple Documents
```vba
Sub ExportMultiple()
    Dim i As Integer
    For i = 1 To 10
        Dim docId As String
        docId = "INV-" & Format(i, "000")
        Call ExportReportSimple("Ispratnica", _
                                "C:\Reports\" & docId & ".pdf", _
                                docId, "Company")
    Next i
End Sub
```

### 3. Export and Email
```vba
Sub ExportAndEmail()
    Dim path As String
    path = Environ("TEMP") & "\invoice.pdf"
    
    If ExportReportSimple("Ispratnica", path, "INV-001", "Company") Then
        ' Use Outlook to send email with attachment
        Dim outlook As Object
        Dim mail As Object
        Set outlook = CreateObject("Outlook.Application")
        Set mail = outlook.CreateItem(0)
        mail.To = "customer@example.com"
        mail.Subject = "Invoice INV-001"
        mail.Attachments.Add path
        mail.Send
    End If
End Sub
```

## Error Handling

```vba
Sub SafeExport()
    On Error GoTo ErrorHandler
    
    Dim success As Boolean
    success = ExportReportSimple("Ispratnica", _
                                  "C:\Reports\invoice.pdf", _
                                  "INV-001", "Company")
    
    If Not success Then
        MsgBox "Export failed. Check the error log.", vbCritical
    End If
    Exit Sub
    
ErrorHandler:
    MsgBox "Error: " & Err.Description, vbCritical
End Sub
```

## Testing Connection

```vba
Sub TestAPI()
    If TestConnection() Then
        MsgBox "API is running!", vbInformation
    Else
        MsgBox "Cannot connect to API!", vbCritical
    End If
End Sub
```

## Response Status Codes

| Code | Description |
|------|-------------|
| 200  | Success - Check response.status for details |
| 400  | Bad Request - Invalid input |
| 499  | Client Closed Request - Request cancelled |
| 500  | Internal Server Error - See error message |

## Tips

1. **Always encode XML to Base64** before sending
2. **Use double backslashes** in file paths for JSON: `"C:\\Reports\\file.pdf"`
3. **Check response.status** even when HTTP status is 200
4. **Enable error handling** in production VBA code
5. **Test with small data** before processing large datasets
6. **Log API calls** for debugging
7. **Handle timeout** for large reports
8. **Validate XML structure** before encoding

## Links

- Full Documentation: [VBA_INTEGRATION_GUIDE.md](VBA_INTEGRATION_GUIDE.md)
- Feature Overview: [SINGLE_REPORT_EXPORT_FEATURE.md](SINGLE_REPORT_EXPORT_FEATURE.md)
- VBA Module: [ReportServiceAPI.bas](ReportServiceAPI.bas)
- HTTP Examples: [single-report-export.http](single-report-export.http)
- Swagger UI: https://localhost:7218/swagger

