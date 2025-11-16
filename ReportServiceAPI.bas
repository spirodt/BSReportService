Attribute VB_Name = "ReportServiceAPI"
' BSReportService VBA API Module
' 
' This module provides easy-to-use functions for calling the BSReportService API
' from VBA applications (Excel, Access, etc.)
'
' Prerequisites:
' - Enable "Microsoft XML, v6.0" reference in VBA Editor (Tools > References)
' - The BSReportService must be running
'
' Usage:
' 1. Import this module into your VBA project
' 2. Update the API_BASE_URL constant if needed
' 3. Call ExportReportSimple or ExportReportWithXml functions
'

Option Explicit

' Configuration
Private Const API_BASE_URL As String = "https://localhost:7218/api/report"

' Main API Functions

' Export a report with simple parameters
' Example: ExportReportSimple("Ispratnica", "C:\Reports\invoice.pdf", "INV-001", "Test Company")
Public Function ExportReportSimple(ByVal reportType As String, _
                                   ByVal outputPath As String, _
                                   ByVal documentId As String, _
                                   Optional ByVal companyName As String = "") As Boolean
    On Error GoTo ErrorHandler
    
    Dim http As Object
    Dim url As String
    Dim requestBody As String
    Dim response As String
    
    ' Create HTTP object
    Set http = CreateObject("MSXML2.XMLHTTP")
    url = API_BASE_URL & "/export-single"
    
    ' Build JSON request
    requestBody = "{" & vbCrLf & _
        """reportType"": """ & reportType & """," & vbCrLf & _
        """outputPath"": """ & EscapeJsonString(outputPath) & """," & vbCrLf & _
        """parameters"": {" & vbCrLf & _
        "  ""DocumentId"": """ & documentId & """"
    
    If companyName <> "" Then
        requestBody = requestBody & "," & vbCrLf & _
            "  ""CompanyName"": """ & companyName & """"
    End If
    
    requestBody = requestBody & vbCrLf & _
        "}" & vbCrLf & _
        "}"
    
    ' Make POST request
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        response = http.responseText
        
        ' Check for API-level errors
        If InStr(response, """status"": ""Error""") > 0 Then
            Dim errorMsg As String
            errorMsg = ExtractJsonValue(response, "errorMessage")
            MsgBox "API Error: " & errorMsg, vbCritical, "Export Failed"
            ExportReportSimple = False
        Else
            ExportReportSimple = True
        End If
    Else
        MsgBox "HTTP Error " & http.Status & ": " & http.responseText, vbCritical, "Export Failed"
        ExportReportSimple = False
    End If
    
    Set http = Nothing
    Exit Function
    
ErrorHandler:
    MsgBox "Error: " & Err.Description, vbCritical, "Export Failed"
    ExportReportSimple = False
End Function

' Export a report with XML data
' Example: ExportReportWithXml("Ispratnica", "C:\Reports\invoice.pdf", xmlData)
Public Function ExportReportWithXml(ByVal reportType As String, _
                                    ByVal outputPath As String, _
                                    ByVal xmlData As String) As Boolean
    On Error GoTo ErrorHandler
    
    Dim http As Object
    Dim url As String
    Dim xmlBase64 As String
    Dim requestBody As String
    Dim response As String
    
    ' Create HTTP object
    Set http = CreateObject("MSXML2.XMLHTTP")
    url = API_BASE_URL & "/export-single"
    
    ' Encode XML to Base64
    xmlBase64 = EncodeBase64(xmlData)
    
    ' Build JSON request
    requestBody = "{" & vbCrLf & _
        """reportType"": """ & reportType & """," & vbCrLf & _
        """outputPath"": """ & EscapeJsonString(outputPath) & """," & vbCrLf & _
        """xmlDataBase64"": """ & xmlBase64 & """" & vbCrLf & _
        "}"
    
    ' Make POST request
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        response = http.responseText
        
        ' Check for API-level errors
        If InStr(response, """status"": ""Error""") > 0 Then
            Dim errorMsg As String
            errorMsg = ExtractJsonValue(response, "errorMessage")
            MsgBox "API Error: " & errorMsg, vbCritical, "Export Failed"
            ExportReportWithXml = False
        Else
            ExportReportWithXml = True
        End If
    Else
        MsgBox "HTTP Error " & http.Status & ": " & http.responseText, vbCritical, "Export Failed"
        ExportReportWithXml = False
    End If
    
    Set http = Nothing
    Exit Function
    
ErrorHandler:
    MsgBox "Error: " & Err.Description, vbCritical, "Export Failed"
    ExportReportWithXml = False
End Function

' Export and get PDF as Base64 string (without saving to file)
Public Function ExportReportGetBase64(ByVal reportType As String, _
                                      ByVal xmlData As String) As String
    On Error GoTo ErrorHandler
    
    Dim http As Object
    Dim url As String
    Dim xmlBase64 As String
    Dim requestBody As String
    Dim response As String
    
    ' Create HTTP object
    Set http = CreateObject("MSXML2.XMLHTTP")
    url = API_BASE_URL & "/export-single"
    
    ' Encode XML to Base64
    xmlBase64 = EncodeBase64(xmlData)
    
    ' Build JSON request (no outputPath)
    requestBody = "{" & vbCrLf & _
        """reportType"": """ & reportType & """," & vbCrLf & _
        """xmlDataBase64"": """ & xmlBase64 & """" & vbCrLf & _
        "}"
    
    ' Make POST request
    http.Open "POST", url, False
    http.setRequestHeader "Content-Type", "application/json"
    http.send requestBody
    
    ' Check response
    If http.Status = 200 Then
        response = http.responseText
        
        ' Check for API-level errors
        If InStr(response, """status"": ""Error""") > 0 Then
            ExportReportGetBase64 = ""
        Else
            ExportReportGetBase64 = ExtractJsonValue(response, "pdfContentBase64")
        End If
    Else
        ExportReportGetBase64 = ""
    End If
    
    Set http = Nothing
    Exit Function
    
ErrorHandler:
    ExportReportGetBase64 = ""
End Function

' Build XML data from Excel range
' Example: BuildXmlFromRange(Range("A1:D10"), "DocumentId", "CompanyName", "Items")
Public Function BuildXmlFromRange(ByVal dataRange As Range, _
                                   ByVal documentIdCell As String, _
                                   ByVal companyNameCell As String, _
                                   ByVal itemsStartRow As Long) As String
    On Error GoTo ErrorHandler
    
    Dim ws As Worksheet
    Dim xmlData As String
    Dim i As Long
    Dim lastRow As Long
    
    Set ws = dataRange.Worksheet
    lastRow = ws.Cells(ws.Rows.Count, dataRange.Column).End(xlUp).Row
    
    ' Build XML header
    xmlData = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf & _
              "<ReportData>" & vbCrLf & _
              "  <Header>" & vbCrLf & _
              "    <DocumentId>" & ws.Range(documentIdCell).Value & "</DocumentId>" & vbCrLf & _
              "    <CompanyName>" & ws.Range(companyNameCell).Value & "</CompanyName>" & vbCrLf & _
              "    <Date>" & Format(Now, "yyyy-mm-dd") & "</Date>" & vbCrLf & _
              "  </Header>" & vbCrLf & _
              "  <Items>" & vbCrLf
    
    ' Add items
    For i = itemsStartRow To lastRow
        If ws.Cells(i, dataRange.Column).Value <> "" Then
            xmlData = xmlData & _
                "    <Item>" & vbCrLf & _
                "      <Name>" & XmlEncode(ws.Cells(i, dataRange.Column).Value) & "</Name>" & vbCrLf & _
                "      <Quantity>" & ws.Cells(i, dataRange.Column + 1).Value & "</Quantity>" & vbCrLf & _
                "      <Price>" & ws.Cells(i, dataRange.Column + 2).Value & "</Price>" & vbCrLf & _
                "    </Item>" & vbCrLf
        End If
    Next i
    
    xmlData = xmlData & _
              "  </Items>" & vbCrLf & _
              "</ReportData>"
    
    BuildXmlFromRange = xmlData
    Exit Function
    
ErrorHandler:
    BuildXmlFromRange = ""
End Function

' Helper Functions

' Encode text to Base64
Private Function EncodeBase64(ByVal text As String) As String
    On Error GoTo ErrorHandler
    
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
    Exit Function
    
ErrorHandler:
    EncodeBase64 = ""
End Function

' Decode Base64 to byte array
Private Function DecodeBase64(ByVal base64String As String) As Byte()
    On Error GoTo ErrorHandler
    
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
    Exit Function
    
ErrorHandler:
    ' Return empty byte array
    Dim emptyArray() As Byte
    DecodeBase64 = emptyArray
End Function

' Save bytes to file
Public Sub SaveBytesToFile(ByRef bytes() As Byte, ByVal filePath As String)
    On Error GoTo ErrorHandler
    
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
    Exit Sub
    
ErrorHandler:
    MsgBox "Error saving file: " & Err.Description, vbCritical
End Sub

' Extract value from JSON string (simple parser)
Private Function ExtractJsonValue(ByVal jsonString As String, ByVal key As String) As String
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

' Escape special characters for JSON
Private Function EscapeJsonString(ByVal text As String) As String
    Dim result As String
    result = text
    
    ' Replace backslashes first
    result = Replace(result, "\", "\\")
    
    ' Replace quotes
    result = Replace(result, """", "\""")
    
    ' Replace special characters
    result = Replace(result, vbCr, "\r")
    result = Replace(result, vbLf, "\n")
    result = Replace(result, vbTab, "\t")
    
    EscapeJsonString = result
End Function

' Encode special XML characters
Private Function XmlEncode(ByVal text As String) As String
    Dim result As String
    result = text
    
    result = Replace(result, "&", "&amp;")
    result = Replace(result, "<", "&lt;")
    result = Replace(result, ">", "&gt;")
    result = Replace(result, """", "&quot;")
    result = Replace(result, "'", "&apos;")
    
    XmlEncode = result
End Function

' Test API connection
Public Function TestConnection() As Boolean
    On Error GoTo ErrorHandler
    
    Dim http As Object
    Dim url As String
    
    Set http = CreateObject("MSXML2.XMLHTTP")
    url = Replace(API_BASE_URL, "/api/report", "/swagger/index.html")
    
    http.Open "GET", url, False
    http.send
    
    If http.Status = 200 Then
        MsgBox "API is accessible and running!", vbInformation, "Connection Test"
        TestConnection = True
    Else
        MsgBox "API returned status: " & http.Status, vbWarning, "Connection Test"
        TestConnection = False
    End If
    
    Set http = Nothing
    Exit Function
    
ErrorHandler:
    MsgBox "Cannot connect to API. Make sure the service is running." & vbCrLf & _
           "Error: " & Err.Description, vbCritical, "Connection Test"
    TestConnection = False
End Function

' Example usage demonstration
Public Sub ExampleUsage()
    ' Example 1: Simple export with parameters
    Dim success As Boolean
    success = ExportReportSimple("Ispratnica", "C:\Reports\test.pdf", "INV-001", "Test Company")
    
    If success Then
        MsgBox "Report exported successfully!", vbInformation
    End If
    
    ' Example 2: Export with XML data
    Dim xmlData As String
    xmlData = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf & _
              "<ReportData>" & vbCrLf & _
              "  <DocumentId>INV-001</DocumentId>" & vbCrLf & _
              "</ReportData>"
    
    success = ExportReportWithXml("Ispratnica", "C:\Reports\test2.pdf", xmlData)
    
    If success Then
        MsgBox "Report with XML data exported successfully!", vbInformation
    End If
End Sub

