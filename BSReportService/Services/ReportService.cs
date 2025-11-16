using BSReportService.BSReports;
using BSReportService.Models;
using DevExpress.ReportServer.ServiceModel.DataContracts;
using System.Data;
using System.Text;
using System.Xml;

namespace BSReportService.Services;

public class ReportService : IReportService
{
    private readonly IReportFactory _reportFactory;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IReportFactory reportFactory, ILogger<ReportService> logger)
    {
        _reportFactory = reportFactory;
        _logger = logger;
    }

    public async Task<ExportReportResponse> ExportReportsToPdfAsync(ReportFilter filter, CancellationToken cancellationToken = default)
    {
        // Get documents based on filter (dummy implementation)
        var documents = await GetDocumentsByFilterAsync(filter, cancellationToken);

        // Export all documents to PDF in parallel
        var exportTasks = documents.Select(doc => ExportDocumentToPdfAsync(doc, cancellationToken));
        var exportedDocuments = await Task.WhenAll(exportTasks);

        return new ExportReportResponse
        {
            Documents = exportedDocuments.ToList(),
            TotalCount = exportedDocuments.Length,
            ExportDate = DateTime.UtcNow
        };
    }

    private async Task<List<ReportDocument>> GetDocumentsByFilterAsync(ReportFilter filter, CancellationToken cancellationToken)
    {
        // Dummy implementation - in real scenario, this would query a database or data source
        await Task.Delay(10, cancellationToken); // Simulate async operation

        // Generate documents based on whether specific ID's are requested and the document type
        var allDocuments = GenerateDummyDocuments(filter.DocumentIds, filter.DocumentType);

        var filteredDocuments = allDocuments.AsQueryable();

        if (filter.StartDate.HasValue)
        {
            filteredDocuments = filteredDocuments.Where(d => d.CreatedDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            filteredDocuments = filteredDocuments.Where(d => d.CreatedDate <= filter.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            filteredDocuments = filteredDocuments.Where(d => d.Status == filter.Status);
        }

        return filteredDocuments.ToList();
    }

    private async Task<ReportDocument> ExportDocumentToPdfAsync(ReportDocument document, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generating PDF for document {DocumentId} of type {DocumentType}", 
                document.DocumentId, document.DocumentType);

            // Create the appropriate report based on document type
            var report = _reportFactory.CreateReport(document.DocumentType);
            
            if (report == null)
            {
                _logger.LogWarning("No report template found for document type {DocumentType}. Using fallback.", 
                    document.DocumentType);
                
                // Return document with null PDF content if no report template exists
                return new ReportDocument
                {
                    DocumentId = document.DocumentId,
                    DocumentType = document.DocumentType,
                    Status = "Error: No report template",
                    CreatedDate = document.CreatedDate,
                    PdfContent = null
                };
            }

            // Set report parameters if the report uses them
            // For example, frmIspratnica has a "Faktura" parameter
            if (report.Parameters.Count > 0)
            {
                // Try to set the first parameter with the document ID
                // This is a simple example - in production, you'd map specific parameters
                var firstParam = report.Parameters[0];
                firstParam.Value = document.DocumentId;
            }

            // Set the data source for the report
            // In a real scenario, this would be fetched from a database
            var reportData = await GetReportDataAsync(document, cancellationToken);
                report.DataSource = reportData;
            report.DataMember = ""; // Assuming the main data member is ""

            // Generate PDF content using DevExpress export
            byte[] pdfContent;
            using (var memoryStream = new MemoryStream())
            {
                // Execute the report to bind data
                await Task.Run(() => report.CreateDocument(), cancellationToken);
                
                // Export to PDF
                await Task.Run(() => report.ExportToPdf(memoryStream), cancellationToken);
                await Task.Run(() => report.ExportToPdf($"C:\\test\\{Guid.NewGuid()}_.pdf"), cancellationToken);
                pdfContent = memoryStream.ToArray();
            }

            _logger.LogInformation("Successfully generated PDF for document {DocumentId}, size: {Size} bytes", 
                document.DocumentId, pdfContent.Length);

            return new ReportDocument
            {
                DocumentId = document.DocumentId,
                DocumentType = document.DocumentType,
                Status = document.Status,
                CreatedDate = document.CreatedDate,
                PdfContent = pdfContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for document {DocumentId}", document.DocumentId);
            
            return new ReportDocument
            {
                DocumentId = document.DocumentId,
                DocumentType = document.DocumentType,
                Status = $"Error: {ex.Message}",
                CreatedDate = document.CreatedDate,
                PdfContent = null
            };
        }
    }

   

    private async Task<object> GetReportDataAsync(ReportDocument document, CancellationToken cancellationToken)
    {
        // This is where you would fetch actual data from your database
        // For now, return dummy data structure that matches the report's expected schema
        
        await Task.Delay(10, cancellationToken); // Simulate async database call

        // Use the helper to create data based on document type
        return document.DocumentType.ToLowerInvariant() switch
        {
            "ispratnica" => ReportDataHelper.CreateIspratnicaMainData(document.DocumentId, document.CreatedDate),
            "invoice" => ReportDataHelper.CreateIspratnicaData(document.DocumentId, document.CreatedDate),
            _ => ReportDataHelper.CreateCustomReportData(document.DocumentId, document.CreatedDate)
        };
    }

    private List<ReportDocument> GenerateDummyDocuments(List<string>? specificIds = null, string? documentType = null)
    {
        // Generate dummy documents for testing
        var documents = new List<ReportDocument>();
        var supportedTypes = _reportFactory.GetSupportedDocumentTypes().ToList();

        // Determine which document type to use
        string GetDocumentType(int index)
        {
            if (!string.IsNullOrEmpty(documentType))
            {
                return documentType; // Use the specified document type
            }
            
            if (supportedTypes.Count > 0)
            {
                return supportedTypes[index % supportedTypes.Count];
            }
            
            return "Unknown";
        }

        // If specific IDs are provided, generate only those documents
        if (specificIds != null && specificIds.Any())
        {
            for (int i = 0; i < specificIds.Count; i++)
            {
                documents.Add(new ReportDocument
                {
                    DocumentId = specificIds[i],
                    DocumentType = GetDocumentType(i),
                    Status = i % 3 == 0 ? "Active" : "Pending",
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                });
            }
        }
        else
        {
            // Generate default set of documents
            for (int i = 1; i <= 10; i++)
            {
                documents.Add(new ReportDocument
                {
                    DocumentId = $"doc{i}",
                    DocumentType = GetDocumentType(i),
                    Status = i % 3 == 0 ? "Active" : "Pending",
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                });
            }
        }

        return documents;
    }

    public async Task<ExportSingleReportResponse> ExportSingleReportToPdfAsync(
        ExportSingleReportRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting single report export for type: {ReportType}", request.ReportType);

            // Create the report based on type
            var report = _reportFactory.CreateReport(request.ReportType);
            if (report == null)
            {
                _logger.LogWarning("Report type '{ReportType}' not found", request.ReportType);
                return new ExportSingleReportResponse
                {
                    ReportType = request.ReportType,
                    Status = "Error",
                    ErrorMessage = $"Report type '{request.ReportType}' not found"
                };
            }

            // Process XML data if provided
            object? reportData = null;
            if (!string.IsNullOrEmpty(request.XmlDataBase64))
            {
                try
                {
                    _logger.LogInformation("Decoding and parsing XML data");
                    var xmlBytes = Convert.FromBase64String(request.XmlDataBase64);
                    var xmlString = Encoding.UTF8.GetString(xmlBytes);
                    
                    // Parse XML to DataSet for DevExpress reports
                    reportData = ParseXmlToDataSet(xmlString);
                    _logger.LogInformation("Successfully parsed XML data");
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Invalid base64 string in XmlDataBase64");
                    throw new FormatException("Invalid base64 string", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse XML data");
                    return new ExportSingleReportResponse
                    {
                        ReportType = request.ReportType,
                        Status = "Error",
                        ErrorMessage = $"Failed to parse XML data: {ex.Message}"
                    };
                }
            }
            else
            {
                // Use parameters to create dummy data if no XML provided
                _logger.LogInformation("No XML data provided, using parameters");
                var documentId = request.Parameters?.GetValueOrDefault("DocumentId") ?? "DEFAULT-001";
                reportData = CreateDataFromParameters(request.ReportType, request.Parameters);
            }

            // Set the data source for the report
            if (reportData != null)
            {
                report.DataSource = reportData;
                report.DataMember = reportData is DataSet ds && ds.Tables.Count > 0 ? ds.Tables[0].TableName : string.Empty;
            }

            // Apply report parameters
            if (request.Parameters != null && report.Parameters.Count > 0)
            {
                foreach (var param in request.Parameters)
                {
                    if (report.Parameters[param.Key] != null)
                    {
                        report.Parameters[param.Key].Value = param.Value;
                        _logger.LogInformation("Set parameter {ParamName} = {ParamValue}", param.Key, param.Value);
                    }
                }
            }

            // Generate PDF
            byte[] pdfContent;
            using (var memoryStream = new MemoryStream())
            {
                // Execute the report to bind data
                await Task.Run(() => report.CreateDocument(), cancellationToken);
                
                // Export to PDF
                await Task.Run(() => report.ExportToPdf(memoryStream), cancellationToken);
                pdfContent = memoryStream.ToArray();
            }

            _logger.LogInformation("Successfully generated PDF, size: {Size} bytes", pdfContent.Length);

            // Save to file if output path is specified
            string? savedPath = null;
            if (!string.IsNullOrEmpty(request.OutputPath))
            {
                try
                {
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(request.OutputPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    await File.WriteAllBytesAsync(request.OutputPath, pdfContent, cancellationToken);
                    savedPath = request.OutputPath;
                    _logger.LogInformation("Saved PDF to: {FilePath}", request.OutputPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save PDF to file: {FilePath}", request.OutputPath);
                    return new ExportSingleReportResponse
                    {
                        ReportType = request.ReportType,
                        Status = "Error",
                        ErrorMessage = $"Failed to save file: {ex.Message}",
                        PdfContent = pdfContent,
                        PdfSizeBytes = pdfContent.Length
                    };
                }
            }

            // Return successful response
            return new ExportSingleReportResponse
            {
                ReportType = request.ReportType,
                PdfContent = pdfContent,
                PdfContentBase64 = Convert.ToBase64String(pdfContent),
                SavedFilePath = savedPath,
                PdfSizeBytes = pdfContent.Length,
                Status = "Success",
                GeneratedDate = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Single report export was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during single report export");
            return new ExportSingleReportResponse
            {
                ReportType = request.ReportType,
                Status = "Error",
                ErrorMessage = $"An error occurred: {ex.Message}"
            };
        }
    }

    private DataSet ParseXmlToDataSet(string xmlString)
    {
        var dataSet = new DataSet();
        
        using (var stringReader = new StringReader(xmlString))
        using (var xmlReader = XmlReader.Create(stringReader))
        {
            dataSet.ReadXml(xmlReader);
        }

        return dataSet;
    }

    private object CreateDataFromParameters(string reportType, Dictionary<string, string>? parameters)
    {
        // Create a simple DataSet with parameters as a table
        var dataSet = new DataSet("ReportData");
        var table = new DataTable("Header");

        // Add columns
        table.Columns.Add("DocumentId", typeof(string));
        table.Columns.Add("CreatedDate", typeof(DateTime));

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                if (!table.Columns.Contains(param.Key))
                {
                    table.Columns.Add(param.Key, typeof(string));
                }
            }
        }

        // Add a row
        var row = table.NewRow();
        row["DocumentId"] = parameters?.GetValueOrDefault("DocumentId") ?? "DEFAULT-001";
        row["CreatedDate"] = DateTime.UtcNow;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                if (param.Key != "DocumentId" && table.Columns.Contains(param.Key))
                {
                    row[param.Key] = param.Value;
                }
            }
        }

        table.Rows.Add(row);
        dataSet.Tables.Add(table);

        // Add an empty items table for reports that expect it
        var itemsTable = new DataTable("Items");
        itemsTable.Columns.Add("ItemId", typeof(string));
        itemsTable.Columns.Add("Name", typeof(string));
        itemsTable.Columns.Add("Quantity", typeof(int));
        itemsTable.Columns.Add("Price", typeof(decimal));
        dataSet.Tables.Add(itemsTable);

        return dataSet;
    }
}
