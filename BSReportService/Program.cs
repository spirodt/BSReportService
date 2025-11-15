using BSReportService.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BS Report Service API",
        Version = "v1",
        Description = "A .NET Web API service for exporting multiple documents to PDF simultaneously with parallel processing capabilities using DevExpress XtraReports.",
        Contact = new OpenApiContact
        {
            Name = "BS Report Service Support",
            Email = "support@bsreportservice.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Register report services
builder.Services.AddSingleton<IReportFactory, ReportFactory>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Configure the HTTP pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BS Report Service API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "BS Report Service API Documentation";
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.EnableFilter();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }
