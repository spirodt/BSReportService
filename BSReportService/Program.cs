using BSReportService.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure OpenAPI with .NET 9 native support
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "BS Report Service API",
            Version = "v1",
            Description = "A .NET 9 Web API service for exporting multiple documents to PDF simultaneously with parallel processing capabilities.",
            Contact = new()
            {
                Name = "BS Report Service Support",
                Email = "support@bsreportservice.com"
            },
            License = new()
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };
        return Task.CompletedTask;
    });
});

// Register report service
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Configure the HTTP pipeline.
// Map OpenAPI endpoint
app.MapOpenApi();

// Configure Swagger UI at /swagger
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "BS Report Service API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "BS Report Service API Documentation";
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.EnableFilter();
});

// Also map Scalar API documentation UI at /scalar (alternative modern UI)
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("BS Report Service API")
        .WithTheme(ScalarTheme.Purple)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }
