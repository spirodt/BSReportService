# GitHub Actions Workflows

This directory contains CI/CD workflows for the BSReportService project.

## Workflows

### 1. CI/CD Pipeline (`ci-cd.yml`)
**Triggers:** Push to main/develop, Pull Requests, Manual dispatch

**Jobs:**
- **Test**: Runs all unit and integration tests
  - Restores NuGet packages using custom `NuGet.config`
  - Builds the solution
  - Executes tests and uploads results as artifacts
  
- **Build Docker**: Builds and tests the Docker image
  - Only runs if tests pass
  - Builds Docker image with caching
  - Performs basic smoke test (checks Swagger endpoint)
  - Saves image as compressed tar artifact
  - Retention: 30 days

**Artifacts:**
- `test-results`: Test execution results (TRX format)
- `docker-image-{sha}`: Compressed Docker image (tar.gz)

### 2. Docker Build and Publish (`docker-build.yml`)
**Triggers:** Push to main/develop, Pull Requests, Tags, Manual dispatch

**Features:**
- Builds Docker image with multi-platform support
- Generates semantic versioning tags
- Saves image as artifact for download
- Creates detailed build summary

**Artifact:**
- `bsreportservice-docker-image-{sha}`: Compressed Docker image

### 3. Publish to GitHub Container Registry (`docker-publish-ghcr.yml`)
**Triggers:** Version tags (v*), Manual dispatch

**Features:**
- Publishes Docker images to GitHub Container Registry (GHCR)
- Automatic semantic versioning from git tags
- Public/private image support
- Generates pull/run commands in summary

**Registry:** `ghcr.io/{owner}/{repo}`

## Usage

### Download and Use Docker Image Artifact

```bash
# Download artifact from GitHub Actions run
# Extract and load the image
gunzip bsreportservice-image.tar.gz
docker load -i bsreportservice-image.tar

# Run the container
docker run -p 8080:8080 bsreportservice:{sha}
```

### Pull from GitHub Container Registry

```bash
# Pull latest version
docker pull ghcr.io/{owner}/bsreportservice:latest

# Pull specific version
docker pull ghcr.io/{owner}/bsreportservice:v1.0.0

# Run
docker run -p 8080:8080 ghcr.io/{owner}/bsreportservice:latest
```

## Permissions

The workflows require the following permissions:
- `contents: read` - Read repository contents
- `packages: write` - Publish to GitHub Container Registry

These are automatically granted via `GITHUB_TOKEN`.

## Secrets

No additional secrets are required. The workflows use the built-in `GITHUB_TOKEN` for authentication.

If you need to access private NuGet feeds during build, add these secrets:
- `NUGET_USERNAME` - Username for private NuGet feed
- `NUGET_TOKEN` - Access token/password for private NuGet feed

Then update the Dockerfile to use build arguments.

## Caching

All workflows use GitHub Actions cache for:
- Docker layer caching (`type=gha`)
- NuGet package caching (implicit in `setup-dotnet`)

This significantly speeds up subsequent builds.

## Customization

### Change .NET Version
Update `DOTNET_VERSION` environment variable in workflows:
```yaml
env:
  DOTNET_VERSION: '8.0.x'
```

### Change Retention Period
Modify `retention-days` in artifact upload steps:
```yaml
- uses: actions/upload-artifact@v4
  with:
    retention-days: 30  # Change this value
```

### Add Additional Tests
Edit the test job in `ci-cd.yml`:
```yaml
- name: Run tests
  run: |
    dotnet test --configuration Release
    # Add more test commands here
```

## Troubleshooting

### NuGet Restore Fails
Ensure `NuGet.config` is present in the repository root and all feeds are accessible from GitHub Actions runners.

### Docker Build Fails
Check that:
- Dockerfile is valid for Linux containers
- All dependencies are available
- NuGet packages can be restored

### GHCR Push Fails
Verify that:
- Repository has packages enabled
- Workflow has `packages: write` permission
- `GITHUB_TOKEN` has not expired

## Monitoring

View workflow runs at:
```
https://github.com/{owner}/{repo}/actions
```

Each run provides:
- Build logs
- Test results
- Artifacts for download
- Summary with usage instructions

