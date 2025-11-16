## Multi-stage Dockerfile to build and run BSReportService as a single-file .NET 8 app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.config if it exists (for local/private feeds)
COPY ["NuGet.config", "./"]

# Copy project files and restore dependencies
COPY ["BSReportService/BSReportService.csproj", "BSReportService/"]
COPY ["BSReportService.Tests/BSReportService.Tests.csproj", "BSReportService.Tests/"]
RUN dotnet restore "BSReportService/BSReportService.csproj" --configfile NuGet.config

# Copy the rest of the source
COPY . .
WORKDIR /src/BSReportService

# Publish as a single-file, self-contained app for Linux x64
# Note: PublishTrimmed is disabled to avoid trimming issues with reflection/third-party libraries.
RUN dotnet publish "BSReportService.csproj" -c Release -r linux-x64 -o /app/publish \
    /p:PublishSingleFile=true \
    /p:SelfContained=true \
    /p:PublishTrimmed=false

# Runtime image (Linux container)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Expose HTTP port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# BSReportService will be published as a single executable (self-contained)
ENTRYPOINT ["./BSReportService"]


