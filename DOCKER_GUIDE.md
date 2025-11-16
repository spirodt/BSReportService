# Docker Deployment Guide

## Overview
The BSReportService is containerized using Docker with single-file publishing for easy deployment.

## Prerequisites
- Docker Desktop (Linux containers mode)
- Docker Compose

## Quick Start

### Using Docker Compose (Recommended)
```bash
# Build and start the service
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the service
docker-compose down
```

### Using Docker CLI
```bash
# Build the image
docker build -t bsreportservice .

# Run the container
docker run -d -p 8080:8080 --name bsreportservice bsreportservice

# View logs
docker logs -f bsreportservice

# Stop and remove
docker stop bsreportservice
docker rm bsreportservice
```

## Configuration

### NuGet Feeds
The `NuGet.config` file is copied during build and contains:
- Official NuGet.org feed
- Custom feed: `https://nuget.epodrum.mk/v3/index.json`

To add credentials for private feeds, use Docker build arguments or update `NuGet.config` before building.

### Application Settings
Mount custom `appsettings.json` via docker-compose volumes:
```yaml
volumes:
  - ./BSReportService/appsettings.Production.json:/app/appsettings.json:ro
```

### Output Directory
Generated reports are saved to `./reports` directory (mounted volume).

## Build Details
- **Base Image**: .NET 8 SDK (build) / ASP.NET 8 Runtime (final)
- **Target Platform**: Linux x64
- **Publishing**: Single-file, self-contained executable
- **Trimming**: Disabled (to support DevExpress and reflection)

## Ports
- **8080**: HTTP API endpoint

## Troubleshooting

### Platform Mismatch Error
Ensure Docker Desktop is running in **Linux containers mode**:
- Right-click Docker Desktop tray icon
- Select "Switch to Linux containers" if on Windows containers

### NuGet Restore Fails
Check that `NuGet.config` is present and feeds are accessible from the build environment.

### Permission Issues
If reports directory has permission issues:
```bash
chmod 777 ./reports
```

## Production Considerations
- Use environment-specific `appsettings.json`
- Configure logging to external sinks (e.g., Seq, Application Insights)
- Set up health checks and monitoring
- Use secrets management for sensitive credentials
- Consider using orchestration (Kubernetes, Docker Swarm) for HA

