using System.Dynamic;

namespace BSReportService.BSReports;

/// <summary>
/// Helper class for creating report data sources.
/// Provides methods to build data structures that match report data bindings.
/// </summary>
public static class ReportDataHelper
{
    public class IspratnicaItem
    {
        public string Artikal { get; set; }
        public string AltSifa { get; set; }
        public string EDM { get; set; }
        public decimal Kolicina { get; set; }
        public string Adresa { get; set; }
        public string Telefon { get; set; }
        public string ImeNaFirma { get; set; }
        public string Naziv { get; set; }
        public string Telefoni { get; set; }
        public string Email { get; set; }
        public string Sifra { get; set; }
        public string EDB { get; set; }
        public string Broj { get; set; }
        public string Datum { get; set; }
        public string Valuta { get; set; }
        public string BankaDeponent { get; set; }
        public string DanocenBroj { get; set; }
        public string Zabeleska { get; set; }
        public byte[] Image1 { get; set; }
    }
    

    public static List<IspratnicaItem> CreateIspratnicaMainData(string documentId, DateTime createdDate)
    {
        var data = new List<IspratnicaItem>();

        for (int i = 1; i <= 3; i++)
        {
            data.Add(new IspratnicaItem
            {
                Artikal = $"Sample Product {i}",
                AltSifa = $"SP00{i}",
                EDM = "pc",
                Kolicina = i * 5,
                Adresa = "123 Main St, Skopje",
                Telefon = "+389 2 123 4567",
                ImeNaFirma = "Sample Company DOOEL",
                Naziv = "Customer Name Ltd.",
                Telefoni = "+389 70 123 456",
                Email = "customer@example.com",
                Sifra = "CUST001",
                EDB = "4080012345678",
                Broj = documentId,
                Datum = createdDate.ToString("dd.MM.yyyy"),
                Valuta = DateTime.Now.AddDays(30).ToString("dd.MM.yyyy"),
                BankaDeponent = "Komercijalna Banka AD Skopje - 300123456789012",
                DanocenBroj = "MK1234567890123",
                Zabeleska = "Sample notes for this delivery note. Please handle with care.",
                Image1 = null
            });
        }

        return data;
    }


    /// <summary>
    /// Creates sample data for Ispratnica (delivery note) report.
    /// </summary>
    public static List<dynamic> CreateIspratnicaData(string documentId, DateTime createdDate)
    {
        var data = new List<dynamic>();

        // Add multiple items to demonstrate detail band
        for (int i = 1; i <= 3; i++)
        {
            dynamic item = new ExpandoObject();
            item.Artikal = $"Sample Product {i}";
            item.AltSifa = $"SP00{i}";
            item.EDM = "pc";
            item.Kolicina = i * 5;
            item.Adresa = "123 Main St, Skopje";
            item.Telefon = "+389 2 123 4567";
            item.ImeNaFirma = "Sample Company DOOEL";
            item.Naziv = "Customer Name Ltd.";
            item.Telefoni = "+389 70 123 456";
            item.Email = "customer@example.com";
            item.Sifra = "CUST001";
            item.EDB = "4080012345678";
            item.Broj = documentId;
            item.Datum = createdDate.ToString("dd.MM.yyyy");
            item.Valuta = DateTime.Now.AddDays(30).ToString("dd.MM.yyyy");
            item.BankaDeponent = "Komercijalna Banka AD Skopje - 300123456789012";
            item.DanocenBroj = "MK1234567890123";
            item.Zabeleska = "Sample notes for this delivery note. Please handle with care.";
            item.Image1 = null; // Can be loaded from file system or database
            
            data.Add(item);
        }

        return data;
    }

    /// <summary>
    /// Creates a generic data structure for custom reports.
    /// Override this method or create similar methods for other report types.
    /// </summary>
    public static List<dynamic> CreateCustomReportData(string documentId, DateTime createdDate, Dictionary<string, object>? customData = null)
    {
        var data = new List<dynamic>();
        
        dynamic item = new ExpandoObject();
        var itemDict = (IDictionary<string, object>)item;
        
        // Add standard fields
        itemDict["DocumentId"] = documentId;
        itemDict["CreatedDate"] = createdDate;
        itemDict["GeneratedDate"] = DateTime.Now;
        
        // Add custom fields if provided
        if (customData != null)
        {
            foreach (var kvp in customData)
            {
                itemDict[kvp.Key] = kvp.Value;
            }
        }
        
        data.Add(item);
        return data;
    }

    /// <summary>
    /// Loads an image from file system for use in reports.
    /// </summary>
    public static byte[]? LoadImageForReport(string imagePath)
    {
        try
        {
            if (File.Exists(imagePath))
            {
                return File.ReadAllBytes(imagePath);
            }
        }
        catch
        {
            // Log error in production
        }
        
        return null;
    }
}

