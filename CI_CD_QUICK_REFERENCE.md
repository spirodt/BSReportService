# CI/CD Quick Reference

## GitHub Actions Workflows Overview

### 🔄 CI/CD Pipeline (Recommended for most use cases)
**File:** `.github/workflows/ci-cd.yml`

**When it runs:**
- Every push to `main` or `develop`
- Every pull request to `main` or `develop`
- Manual trigger via GitHub UI

**What it does:**
1. ✅ Runs all tests (unit + integration)
2. 🐳 Builds Docker image
3. 🧪 Smoke tests the image (checks Swagger endpoint)
4. 📦 Saves image as artifact

**Artifacts produced:**
- `test-results` - Test execution results (TRX format)
- `docker-image-{sha}` - Compressed Docker image (tar.gz)

---

### 🐋 Docker Build Only
**File:** `.github/workflows/docker-build.yml`

**When it runs:**
- Push to `main`/`develop`
- Pull requests
- Git tags (`v*`)
- Manual trigger

**What it does:**
- Builds Docker image with semantic versioning
- Saves as downloadable artifact

---

### 📦 Publish to GitHub Container Registry
**File:** `.github/workflows/docker-publish-ghcr.yml`

**When it runs:**
- Git tags starting with `v` (e.g., `v1.0.0`)
- Manual trigger

**What it does:**
- Builds and publishes to `ghcr.io`
- Creates version tags automatically
- Makes image publicly/privately available

---

## How to Use Artifacts

### Download Docker Image from GitHub Actions

1. Go to the Actions tab in your repository
2. Click on a successful workflow run
3. Scroll to "Artifacts" section
4. Download `docker-image-{sha}.tar.gz`

### Load and Run the Image

```bash
# Extract the compressed image
gunzip docker-image-abc123.tar.gz

# Load into Docker
docker load -i docker-image-abc123.tar

# List loaded images
docker images

# Run the container
docker run -d -p 8080:8080 --name bsreportservice bsreportservice:abc123

# Access the API
curl http://localhost:8080/swagger/index.html
```

---

## How to Publish a Release

### Option 1: Using Git Tags (Automated)

```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0

# This triggers:
# - Docker build
# - Publish to GHCR (ghcr.io/owner/repo:v1.0.0)
```

### Option 2: Manual Trigger

1. Go to Actions tab
2. Select "Publish to GitHub Container Registry"
3. Click "Run workflow"
4. Select branch and click "Run workflow"

---

## Pull from GitHub Container Registry

```bash
# Pull latest version
docker pull ghcr.io/YOUR_ORG/bsreportservice:latest

# Pull specific version
docker pull ghcr.io/YOUR_ORG/bsreportservice:v1.0.0

# Run
docker run -p 8080:8080 ghcr.io/YOUR_ORG/bsreportservice:latest
```

---

## Troubleshooting

### Tests Fail
- Check test results artifact for details
- Review logs in the "Run tests" step
- Ensure NuGet packages restore correctly

### Docker Build Fails
**Common causes:**
- NuGet restore issues (check `NuGet.config`)
- Missing dependencies
- Dockerfile syntax errors

**Solution:**
- Test locally first: `docker build -t test .`
- Check workflow logs for specific error

### GHCR Push Fails
**Common causes:**
- Missing `packages: write` permission
- Repository packages not enabled
- Authentication issues

**Solution:**
- Verify repository settings → Actions → General → Workflow permissions
- Ensure "Read and write permissions" is enabled

### Artifact Download Issues
- Artifacts expire after 30 days (configurable)
- Check retention settings in workflow files
- Ensure workflow completed successfully

---

## Configuration

### Change .NET Version
Edit workflow files:
```yaml
env:
  DOTNET_VERSION: '8.0.x'  # Change this
```

### Change Artifact Retention
Edit workflow files:
```yaml
- uses: actions/upload-artifact@v4
  with:
    retention-days: 30  # Change this (1-90 days)
```

### Add Private NuGet Feed Credentials
Add repository secrets:
- `NUGET_USERNAME`
- `NUGET_TOKEN`

Update Dockerfile to use build args (see `DOCKER_GUIDE.md`).

---

## Monitoring

### View Workflow Status
```
https://github.com/YOUR_ORG/BSReportService/actions
```

### Check Build Summary
Each workflow run includes a summary with:
- Build status
- Test results
- Image details
- Usage commands

---

## Best Practices

1. **Always run tests locally before pushing:**
   ```bash
   dotnet test
   ```

2. **Test Docker build locally:**
   ```bash
   docker-compose build
   docker-compose up
   ```

3. **Use semantic versioning for tags:**
   - `v1.0.0` - Major release
   - `v1.1.0` - Minor release (new features)
   - `v1.1.1` - Patch release (bug fixes)

4. **Review PR checks before merging:**
   - All tests must pass
   - Docker build must succeed

5. **Keep artifacts organized:**
   - Download important releases
   - Archive critical versions externally

---

## Quick Commands Cheat Sheet

```bash
# Local development
docker-compose up -d              # Start service
docker-compose logs -f            # View logs
docker-compose down               # Stop service

# Load artifact
gunzip image.tar.gz
docker load -i image.tar

# Pull from registry
docker pull ghcr.io/org/repo:tag

# Run container
docker run -p 8080:8080 image:tag

# Create release
git tag v1.0.0
git push origin v1.0.0

# Test locally
dotnet test
docker build -t test .
```

---

## Support

For detailed documentation:
- **Docker setup**: See `DOCKER_GUIDE.md`
- **Workflows**: See `.github/workflows/README.md`
- **API usage**: See `README.md`

