# VBA Integration Guide for BSReportService

This guide provides comprehensive examples for calling the BSReportService API from VBA applications (Excel, Access, etc.) to export reports with XML data.

## Prerequisites

1. Enable "Microsoft XML, v6.0" reference in VBA Editor (Tools > References)
2. The BSReportService must be running (default: https://localhost:7218)

## Basic Example: Export Single Report with Parameters

```vba
Sub ExportReportWithParameters()
    Dim http As Object
    Dim url As String
    Dim requestBody As String
    Dim response As String
    
    ' Create HTTP object
    Set http = CreateObject("MSXML2.XMLHTTP")
    
    ' API endpoint
    url = "https://localhost:7218/api/report/export-single"
    
    ' Build JSON request with parameters
    requestBody = "{" & vbCrLf & _
        """reportType"": ""Ispratnica""," & vbCrLf & _
        """outputPath"": ""C:\\Reports\\Invoice_001.pdf""," & vbCrLf & _
        """parameters"": {" & vbCrLf & _
        "  ""DocumentId"": ""INV-001""," & vbCrLf & _
        "  ""CompanyName"": ""Test Company""" & vbCrLf & _
        "}" & vbCrLf & _
        "}"
    
    ' Make POST request
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        response = http.responseText
        MsgBox "Report exported successfully!" & vbCrLf & response, vbInformation
    Else
        MsgBox "Error: " & http.Status & " - " & http.responseText, vbCritical
    End If
    
    Set http = Nothing
End Sub
```

## Advanced Example: Export with XML Data (Base64 Encoded)

```vba
Sub ExportReportWithXmlData()
    Dim http As Object
    Dim url As String
    Dim xmlData As String
    Dim xmlBase64 As String
    Dim requestBody As String
    Dim response As String
    Dim jsonResponse As Object
    
    ' Create HTTP object
    Set http = CreateObject("MSXML2.XMLHTTP")
    
    ' API endpoint
    url = "https://localhost:7218/api/report/export-single"
    
    ' Build XML data
    xmlData = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf & _
              "<ReportData>" & vbCrLf & _
              "  <Header>" & vbCrLf & _
              "    <DocumentId>INV-001</DocumentId>" & vbCrLf & _
              "    <CompanyName>Test Company Ltd.</CompanyName>" & vbCrLf & _
              "    <Date>2024-01-15</Date>" & vbCrLf & _
              "  </Header>" & vbCrLf & _
              "  <Items>" & vbCrLf & _
              "    <Item>" & vbCrLf & _
              "      <Name>Product 1</Name>" & vbCrLf & _
              "      <Quantity>10</Quantity>" & vbCrLf & _
              "      <Price>100.00</Price>" & vbCrLf & _
              "    </Item>" & vbCrLf & _
              "    <Item>" & vbCrLf & _
              "      <Name>Product 2</Name>" & vbCrLf & _
              "      <Quantity>5</Quantity>" & vbCrLf & _
              "      <Price>50.00</Price>" & vbCrLf & _
              "    </Item>" & vbCrLf & _
              "  </Items>" & vbCrLf & _
              "</ReportData>"
    
    ' Encode XML to Base64
    xmlBase64 = EncodeBase64(xmlData)
    
    ' Build JSON request
    requestBody = "{" & vbCrLf & _
        """reportType"": ""Ispratnica""," & vbCrLf & _
        """outputPath"": ""C:\\Reports\\Invoice_001.pdf""," & vbCrLf & _
        """xmlDataBase64"": """ & xmlBase64 & """" & vbCrLf & _
        "}"
    
    ' Make POST request
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        response = http.responseText
        
        ' Parse JSON response
        Set jsonResponse = ParseJson(response)
        
        If jsonResponse("status") = "Success" Then
            MsgBox "Report exported successfully!" & vbCrLf & _
                   "File saved to: " & jsonResponse("savedFilePath") & vbCrLf & _
                   "PDF Size: " & jsonResponse("pdfSizeBytes") & " bytes", vbInformation
        Else
            MsgBox "Error: " & jsonResponse("errorMessage"), vbCritical
        End If
    Else
        MsgBox "HTTP Error: " & http.Status & " - " & http.responseText, vbCritical
    End If
    
    Set http = Nothing
    Set jsonResponse = Nothing
End Sub
```

## Helper Function: Base64 Encoding

```vba
Function EncodeBase64(ByVal text As String) As String
    Dim arrData() As Byte
    Dim objXML As Object
    Dim objNode As Object
    
    ' Convert text to byte array
    arrData = StrConv(text, vbFromUnicode)
    
    ' Create XML DOM object
    Set objXML = CreateObject("MSXML2.DOMDocument")
    Set objNode = objXML.createElement("b64")
    
    ' Encode to Base64
    objNode.DataType = "bin.base64"
    objNode.nodeTypedValue = arrData
    EncodeBase64 = objNode.text
    
    Set objNode = Nothing
    Set objXML = Nothing
End Function
```

## Helper Function: Base64 Decoding (for receiving PDF content)

```vba
Function DecodeBase64(ByVal base64String As String) As Byte()
    Dim objXML As Object
    Dim objNode As Object
    
    ' Create XML DOM object
    Set objXML = CreateObject("MSXML2.DOMDocument")
    Set objNode = objXML.createElement("b64")
    
    ' Decode from Base64
    objNode.DataType = "bin.base64"
    objNode.text = base64String
    DecodeBase64 = objNode.nodeTypedValue
    
    Set objNode = Nothing
    Set objXML = Nothing
End Function
```

## Complete Example: Export and Save PDF from Response

```vba
Sub ExportAndSavePdfFromResponse()
    Dim http As Object
    Dim url As String
    Dim xmlData As String
    Dim xmlBase64 As String
    Dim requestBody As String
    Dim response As String
    Dim pdfBase64 As String
    Dim pdfBytes() As Byte
    Dim outputPath As String
    
    ' Create HTTP object
    Set http = CreateObject("MSXML2.XMLHTTP")
    
    ' API endpoint
    url = "https://localhost:7218/api/report/export-single"
    
    ' Build simple XML data
    xmlData = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf & _
              "<ReportData>" & vbCrLf & _
              "  <DocumentId>INV-001</DocumentId>" & vbCrLf & _
              "</ReportData>"
    
    ' Encode XML to Base64
    xmlBase64 = EncodeBase64(xmlData)
    
    ' Build JSON request (without outputPath to get PDF in response)
    requestBody = "{" & vbCrLf & _
        """reportType"": ""Ispratnica""," & vbCrLf & _
        """xmlDataBase64"": """ & xmlBase64 & """" & vbCrLf & _
        "}"
    
    ' Make POST request
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        response = http.responseText
        
        ' Extract PDF Base64 from JSON response (simplified parsing)
        pdfBase64 = ExtractJsonValue(response, "pdfContentBase64")
        
        ' Decode Base64 to bytes
        pdfBytes = DecodeBase64(pdfBase64)
        
        ' Save to file
        outputPath = "C:\Reports\Invoice_" & Format(Now, "yyyymmdd_hhnnss") & ".pdf"
        SaveBytesToFile pdfBytes, outputPath
        
        MsgBox "PDF saved successfully to: " & outputPath, vbInformation
    Else
        MsgBox "HTTP Error: " & http.Status & " - " & http.responseText, vbCritical
    End If
    
    Set http = Nothing
End Sub
```

## Helper Function: Save Bytes to File

```vba
Sub SaveBytesToFile(ByRef bytes() As Byte, ByVal filePath As String)
    Dim stream As Object
    
    ' Create ADODB Stream object
    Set stream = CreateObject("ADODB.Stream")
    
    ' Configure stream
    stream.Type = 1 ' adTypeBinary
    stream.Open
    stream.Write bytes
    
    ' Save to file
    stream.SaveToFile filePath, 2 ' adSaveCreateOverWrite
    stream.Close
    
    Set stream = Nothing
End Sub
```

## Helper Function: Simple JSON Value Extraction

```vba
Function ExtractJsonValue(ByVal jsonString As String, ByVal key As String) As String
    Dim startPos As Long
    Dim endPos As Long
    Dim searchKey As String
    
    ' Search for the key
    searchKey = """" & key & """:"
    startPos = InStr(jsonString, searchKey)
    
    If startPos > 0 Then
        startPos = startPos + Len(searchKey)
        
        ' Skip whitespace and opening quote
        Do While Mid(jsonString, startPos, 1) = " " Or Mid(jsonString, startPos, 1) = """"
            startPos = startPos + 1
        Loop
        
        ' Find closing quote
        endPos = InStr(startPos, jsonString, """")
        
        If endPos > 0 Then
            ExtractJsonValue = Mid(jsonString, startPos, endPos - startPos)
        End If
    End If
End Function
```

## Example: Export from Excel Data

```vba
Sub ExportExcelDataToReport()
    Dim http As Object
    Dim url As String
    Dim xmlData As String
    Dim xmlBase64 As String
    Dim requestBody As String
    Dim ws As Worksheet
    Dim lastRow As Long
    Dim i As Long
    
    ' Set worksheet
    Set ws = ThisWorkbook.Sheets("InvoiceData")
    lastRow = ws.Cells(ws.Rows.Count, "A").End(xlUp).Row
    
    ' Build XML from Excel data
    xmlData = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf & _
              "<ReportData>" & vbCrLf & _
              "  <Header>" & vbCrLf & _
              "    <DocumentId>" & ws.Range("B2").Value & "</DocumentId>" & vbCrLf & _
              "    <CompanyName>" & ws.Range("B3").Value & "</CompanyName>" & vbCrLf & _
              "    <Date>" & Format(ws.Range("B4").Value, "yyyy-mm-dd") & "</Date>" & vbCrLf & _
              "  </Header>" & vbCrLf & _
              "  <Items>" & vbCrLf
    
    ' Add items from rows
    For i = 7 To lastRow
        If ws.Cells(i, 1).Value <> "" Then
            xmlData = xmlData & _
                "    <Item>" & vbCrLf & _
                "      <Name>" & ws.Cells(i, 1).Value & "</Name>" & vbCrLf & _
                "      <Quantity>" & ws.Cells(i, 2).Value & "</Quantity>" & vbCrLf & _
                "      <Price>" & ws.Cells(i, 3).Value & "</Price>" & vbCrLf & _
                "    </Item>" & vbCrLf
        End If
    Next i
    
    xmlData = xmlData & _
              "  </Items>" & vbCrLf & _
              "</ReportData>"
    
    ' Encode XML to Base64
    xmlBase64 = EncodeBase64(xmlData)
    
    ' Create HTTP object
    Set http = CreateObject("MSXML2.XMLHTTP")
    url = "https://localhost:7218/api/report/export-single"
    
    ' Build JSON request
    requestBody = "{" & vbCrLf & _
        """reportType"": ""Ispratnica""," & vbCrLf & _
        """outputPath"": ""C:\\Reports\\Invoice_" & ws.Range("B2").Value & ".pdf""," & vbCrLf & _
        """xmlDataBase64"": """ & xmlBase64 & """" & vbCrLf & _
        "}"
    
    ' Make POST request
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        MsgBox "Invoice exported successfully!", vbInformation
    Else
        MsgBox "Error: " & http.Status & " - " & http.responseText, vbCritical
    End If
    
    Set http = Nothing
    Set ws = Nothing
End Sub
```

## Error Handling Best Practices

```vba
Sub ExportWithErrorHandling()
    On Error GoTo ErrorHandler
    
    Dim http As Object
    Dim url As String
    Dim requestBody As String
    Dim response As String
    
    Set http = CreateObject("MSXML2.XMLHTTP")
    url = "https://localhost:7218/api/report/export-single"
    
    requestBody = "{""reportType"": ""Ispratnica""}"
    
    ' Make POST request with timeout
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        response = http.responseText
        
        ' Check if the API returned an error in the response body
        If InStr(response, """status"": ""Error""") > 0 Then
            Dim errorMsg As String
            errorMsg = ExtractJsonValue(response, "errorMessage")
            MsgBox "API Error: " & errorMsg, vbCritical
        Else
            MsgBox "Report exported successfully!", vbInformation
        End If
    Else
        MsgBox "HTTP Error " & http.Status & ": " & http.responseText, vbCritical
    End If
    
ExitSub:
    Set http = Nothing
    Exit Sub
    
ErrorHandler:
    MsgBox "VBA Error " & Err.Number & ": " & Err.Description, vbCritical
    Resume ExitSub
End Sub
```

## Testing the Connection

```vba
Sub TestApiConnection()
    Dim http As Object
    Dim url As String
    
    On Error GoTo ErrorHandler
    
    Set http = CreateObject("MSXML2.XMLHTTP")
    url = "https://localhost:7218/swagger/index.html"
    
    http.Open "GET", url, False
    http.send
    
    If http.Status = 200 Then
        MsgBox "API is accessible!", vbInformation
    Else
        MsgBox "API returned status: " & http.Status, vbWarning
    End If
    
    Set http = Nothing
    Exit Sub
    
ErrorHandler:
    MsgBox "Cannot connect to API. Make sure the service is running." & vbCrLf & _
           "Error: " & Err.Description, vbCritical
End Sub
```

## API Response Structure

The API returns a JSON response with the following structure:

```json
{
  "reportType": "Ispratnica",
  "pdfContent": [byte array],
  "pdfContentBase64": "JVBERi0xLjQKJcO...",
  "savedFilePath": "C:\\Reports\\Invoice_001.pdf",
  "pdfSizeBytes": 102400,
  "generatedDate": "2024-12-14T12:00:00Z",
  "status": "Success",
  "errorMessage": null
}
```

## Tips for VBA Integration

1. **Use Base64 Encoding**: Always encode XML data to Base64 before sending to ensure special characters are handled correctly.

2. **Error Handling**: Always implement proper error handling in VBA as network operations can fail.

3. **File Paths**: Use double backslashes (`\\`) in JSON strings for Windows file paths.

4. **Large Data**: For large XML data (>100KB), consider breaking it into smaller chunks or optimizing the structure.

5. **Async Calls**: For production use, consider implementing asynchronous HTTP calls to prevent Excel from freezing.

6. **SSL Certificates**: If using HTTPS in production, ensure proper SSL certificate configuration.

7. **Authentication**: Add authentication headers if your API requires them:
   ```vba
   http.setRequestHeader "Authorization", "Bearer YOUR_TOKEN"
   ```

8. **Logging**: Implement logging to track API calls and responses for debugging.

## Common Issues and Solutions

### Issue: "Run-time error '-2147012889': The certificate authority is invalid or incorrect"
**Solution**: This occurs with self-signed certificates. For development:
```vba
' Add this before http.send
http.setOption 2, 13056 ' Ignore SSL errors (development only!)
```

### Issue: "Object required" error
**Solution**: Ensure all required VBA references are enabled in Tools > References.

### Issue: API returns 400 Bad Request
**Solution**: Verify your JSON structure is correct. Use online JSON validators.

### Issue: PDF file is corrupted
**Solution**: Ensure Base64 decoding is working correctly. Test with small files first.

## Next Steps

1. Test the basic example first
2. Adapt the XML structure to match your report's expected schema
3. Implement error handling for production use
4. Consider creating a VBA class module for reusable API functionality

For more information about the API endpoints, visit the Swagger documentation at:
https://localhost:7218/swagger

