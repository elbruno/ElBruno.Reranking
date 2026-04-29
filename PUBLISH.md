# Publishing to NuGet

This guide explains how to publish a new version of ElBruno.Reranking to NuGet.org.

## Overview

The publish process is **fully automated** via GitHub Actions. Once you create a GitHub Release, the workflow:
1. Extracts the version from the release tag
2. Runs tests
3. Builds the package with your README included
4. Publishes to NuGet.org
5. Uploads package artifacts for reference

**Total time:** 5-10 minutes from release creation to package availability on NuGet.org

---

## Prerequisites

Before you can publish, ensure:

- ✅ You have commit access to this repository
- ✅ The `.csproj` file has the correct version number
- ✅ Your changes are committed and pushed to `main`
- ✅ README.md exists and includes the `PackageReadmeFile` property in the `.csproj` (automatic since v0.5.1)

---

## Publishing Steps

### Step 1: Update Version in .csproj

Edit `src/ElBruno.Reranking/ElBruno.Reranking.csproj`:

```xml
<PropertyGroup>
  <Version>0.5.1</Version>  <!-- Update this -->
  ...
</PropertyGroup>
```

Use semantic versioning:
- **Patch** (0.5.1): Bug fixes, documentation updates
- **Minor** (0.6.0): New features, backward compatible
- **Major** (1.0.0): Breaking changes

### Step 2: Commit Changes

```bash
git add src/ElBruno.Reranking/ElBruno.Reranking.csproj
git commit -m "docs: update version to 0.5.1"
# Or use a more descriptive message:
git commit -m "feat: add new feature and bump to 0.6.0"
git push origin main
```

### Step 3: Create Git Tag

Create an annotated tag matching the version:

```bash
git tag -a v0.5.1 -m "Release v0.5.1: Add README to NuGet package"
git push origin main --tags
```

**Tag format:** `v{MAJOR}.{MINOR}.{PATCH}` (must start with 'v')

### Step 4: Create GitHub Release

```bash
gh release create v0.5.1 \
  --title "v0.5.1: Full NuGet Documentation" \
  --notes "Release notes here..."
```

Or use the GitHub UI: https://github.com/elbruno/ElBruno.Reranking/releases

**Release notes template:**
```markdown
## Changes
- [x] Feature/fix description
- [x] Another change

## Highlights
- What's new or improved

## Breaking Changes
- None (or list if applicable)
```

---

## What Happens Next

Once you create the release:

1. **GitHub detects the `release.published` event** → triggers `.github/workflows/publish.yml`

2. **Workflow execution** (automated):
   - Extracts version: `v0.5.1` → `0.5.1`
   - Runs all tests in Release mode
   - Builds the NuGet package (`.nupkg`)
   - Includes README.md automatically
   - Authenticates with NuGet.org via OIDC
   - Pushes package to https://api.nuget.org/v3/index.json
   - Uploads package artifact to GitHub

3. **Verification**:
   - Check workflow status: https://github.com/elbruno/ElBruno.Reranking/actions
   - Package appears on NuGet: https://www.nuget.org/packages/ElBruno.Reranking/
   - README displays on the package page (after ~5 min)

---

## Workflow Details

### Trigger Event
```yaml
on:
  release:
    types: [published, released]
```

The workflow triggers when a GitHub Release is **published** (not saved as draft).

### Version Extraction
```bash
VERSION="${{ github.event.release.tag_name }}"
VERSION="${VERSION#v}"  # Strips 'v' prefix
# Result: v0.5.1 → 0.5.1
```

### Key Steps

| Step | What it does | Command |
|------|-------------|---------|
| **Determine version** | Extracts from release tag | `github.event.release.tag_name` |
| **Restore** | Downloads NuGet packages | `dotnet restore` |
| **Build** | Compiles in Release mode | `dotnet build -c Release -p:Version=0.5.1` |
| **Test** | Runs all unit tests | `dotnet test -c Release` |
| **Pack** | Creates .nupkg file | `dotnet pack -c Release -o artifacts/` |
| **Authenticate** | Logs into NuGet.org | OIDC (secure, no API key) |
| **Push** | Publishes to NuGet | `dotnet nuget push *.nupkg` |
| **Upload Artifact** | Stores .nupkg for download | GitHub Actions artifact |

### Package Metadata

Your package includes:

- ✅ README.md (displayed on NuGet page)
- ✅ LICENSE file
- ✅ All source files in `src/ElBruno.Reranking/`
- ✅ Version number (from .csproj)
- ✅ Description, authors, project URL (from .csproj)

---

## Troubleshooting

### Package not appearing on NuGet.org

**Issue:** Released successfully, but package doesn't show up

**Solutions:**
1. Wait 5-10 minutes (NuGet can take time to sync)
2. Check workflow status in Actions tab — look for ❌ failures
3. If workflow failed, check logs for error messages
4. Verify version number matches the tag exactly

### Workflow Failed

**Check logs:**
1. Go to https://github.com/elbruno/ElBruno.Reranking/actions
2. Click the failed workflow run
3. Expand the failed step and read the error

**Common errors:**
- **Tests failed:** Run `dotnet test` locally to debug
- **Version mismatch:** Ensure .csproj version matches git tag
- **Authentication failed:** OIDC issue (contact repo maintainer)

### Cannot create release

**Issue:** `gh release create` returns "not authenticated"

**Solution:**
```bash
gh auth login
# Follow the prompts to authenticate with GitHub
```

---

## Manual Publishing (Emergency Only)

If the automated workflow fails and you need to publish manually:

```bash
cd src/ElBruno.Reranking
dotnet pack -c Release -p:Version=0.5.1 -o ./artifacts/
dotnet nuget push ./artifacts/ElBruno.Reranking.0.5.1.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

⚠️ Get your NuGet API key from https://www.nuget.org/account/apikeys

---

## FAQ

**Q: Can I publish without a GitHub Release?**
A: No. The workflow only triggers on `release.published`. You must create a release through GitHub.

**Q: Does the tag have to match the version in .csproj?**
A: Not exactly, but it should be close. The workflow extracts version from the tag, then uses it during build. Best practice: keep them in sync.

**Q: What if I need to unpublish a version?**
A: NuGet.org allows listing a package as unlisted (hidden from search) but doesn't allow deletion. Contact NuGet support if needed.

**Q: Can I schedule automatic releases?**
A: Yes, but it's not currently configured. You can edit `.github/workflows/publish.yml` to add a schedule trigger if desired.

**Q: Where's the API key stored?**
A: Not stored. The workflow uses OIDC (OpenID Connect) for authentication — no secrets in the repo.

---

## Related Files

| File | Purpose |
|------|---------|
| `.github/workflows/publish.yml` | Defines the automated publish workflow |
| `src/ElBruno.Reranking/ElBruno.Reranking.csproj` | Project metadata, version, package properties |
| `README.md` | Package documentation (included in .nupkg) |
| `LICENSE` | Package license file |

---

## Support

For issues or questions:
- 📧 Check GitHub Issues: https://github.com/elbruno/ElBruno.Reranking/issues
- 🔍 Review workflow logs: https://github.com/elbruno/ElBruno.Reranking/actions
- 📦 Check NuGet page: https://www.nuget.org/packages/ElBruno.Reranking/
