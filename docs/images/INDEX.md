# ElBruno.Reranking v0.5.0 Promotional Images

## Generated Assets

This directory contains promotional images for ElBruno.Reranking v0.5.0 release.

### NuGet Package Icon (Multi-size)

| Image | Dimensions | File Size | Format | Special Notes |
|-------|-----------|-----------|--------|---------------|
| nuget-icon-128x128.png | 128×128 px | ~915 KB | PNG | Primary NuGet package icon, transparent background |
| nuget-icon-64x64.png | 64×64 px | ~716 KB | PNG | Small display variant, transparent background |
| nuget-icon-32x32.png | 32×32 px | ~682 KB | PNG | Favicon/toolbar variant, transparent background |

**Specifications:**
- Format: PNG with transparent background
- Colors: Blue, white, and purple (modern tech palette)
- Design: Sleek, minimalist with magnifying glass/ranking symbol
- Recognizable at small sizes

---

### Blog & Documentation Images

| Image | Dimensions | File Size | Purpose | Format |
|-------|-----------|-----------|---------|--------|
| blog-hero-1200x630.png | 1200×630 px | ~763 KB | Blog post header | PNG |
| docs-hero-1920x400.png | 1920×400 px | ~??? KB | Documentation page header | PNG |

---

### Social Media Images

| Image | Dimensions | File Size | Platform | Purpose | Format |
|-------|-----------|-----------|----------|---------|--------|
| linkedin-promo-1200x627.png | 1200×627 px | ~??? KB | LinkedIn | Professional announcement with metrics | PNG |
| twitter-announcement-1024x512.png | 1024×512 px | ~??? KB | Twitter/X | Attention-grabbing announcement | PNG |
| github-social-preview-1280x640.png | 1280×640 px | ~??? KB | GitHub | Repository social preview card | PNG |
| youtube-thumbnail-1280x720.png | 1280×720 px | ~??? KB | YouTube | Video thumbnail | PNG |

---

### Carousel Slides (LinkedIn/Twitter)

| Image | Dimensions | File Size | Content | Format |
|-------|-----------|-----------|---------|--------|
| carousel-slide-1-1080x1350.png | 1080×1350 px | ~??? KB | Title slide - Introducing ElBruno.Reranking | PNG |
| carousel-slide-2-1080x1350.png | 1080×1350 px | ~??? KB | ONNX Backend benefits (15ms, free, local) | PNG |
| carousel-slide-3-1080x1350.png | 1080×1350 px | ~??? KB | Claude API benefits (98% accuracy, reasoning) | PNG |
| carousel-slide-4-1080x1350.png | 1080×1350 px | PENDING | Performance comparison metrics | PNG |
| carousel-slide-5-1080x1350.png | 1080×1350 px | PENDING | Call-to-action (NuGet, GitHub, Docs) | PNG |

---

### Marketing & Comparison

| Image | Dimensions | File Size | Purpose | Format |
|-------|-----------|-----------|---------|--------|
| comparison-chart-1200x800.png | 1200×800 px | ~??? KB | Backend comparison infographic | PNG |
| release-banner-600x300.png | 600×300 px | PENDING | v1.0 release announcement banner | PNG |

---

## Generation Details

**Tool:** t2i CLI (Microsoft Foundry Image-2 provider)  
**Generation Date:** 2026-04-28  
**Status:** 13 of 16 images generated successfully

### Successfully Generated (13 images):
- ✅ All 3 NuGet icon sizes (128×128, 64×64, 32×32)
- ✅ Blog hero image
- ✅ LinkedIn promotion graphic
- ✅ Twitter announcement image
- ✅ GitHub social preview
- ✅ Documentation hero
- ✅ Comparison chart
- ✅ YouTube thumbnail
- ✅ Carousel slides 1-3 (title, ONNX, Claude)

### Pending Generation (3 images):
- ⏳ Carousel slide 4 (performance comparison)
- ⏳ Carousel slide 5 (call-to-action)
- ⏳ Release banner (v1.0 announcement)

---

## Branding Specifications

### Color Palette
- Primary Blue: #0078D4
- Secondary Purple: #6C3FB5
- White: #FFFFFF
- Tech Accents: Cyan, bright green

### Typography
- Modern, clean fonts
- High-contrast text for accessibility
- Readable at 50% zoom

### Design Principles
- Consistent visual language across all assets
- Icons and elements follow unified style
- Tech-focused, professional aesthetic
- Optimized for both dark and light backgrounds (where applicable)

---

## Usage Guide

### For Blog Posts
Use `blog-hero-1200x630.png` as header image in Markdown frontmatter:
```markdown
---
hero_image: docs/images/blog-hero-1200x630.png
---
```

### For Social Media
- **LinkedIn:** Use `linkedin-promo-1200x627.png` or carousel slides
- **Twitter:** Use `twitter-announcement-1024x512.png`
- **GitHub:** Automatic via social preview image in repository settings
- **YouTube:** Use `youtube-thumbnail-1280x720.png` for video thumbs

### For NuGet Package
Upload `nuget-icon-128x128.png` to NuGet Package Manager as the package icon.

### For Documentation
Use `docs-hero-1920x400.png` at the top of documentation pages.

---

## Technical Notes

- All images generated as PNG format with transparency where specified
- Dimensions are optimized for platform-specific requirements
- High-resolution assets suitable for web and print use
- Generated with AI image model (foundry-mai2) for consistency

---

## Integration with Promotion Content

These images are referenced in:
- `docs/promotion/blog-post-reranking-announcement.md` - Hero image
- `docs/promotion/linkedin-post.md` - Social media graphics
- `docs/promotion/twitter-post.md` - Twitter announcement
- GitHub Repository Settings → Social Preview

