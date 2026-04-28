# ElBruno.Reranking Library PRD - Repository & Promotion Addendum

## Additional Directives for Both Libraries

Based on user requirements, this addendum applies to **both** ElBruno.BM25 and ElBruno.Reranking.

---

## Repository Structure

### GitHub Repository Setup
- **Repository:** `github.com/elbruno/ElBruno.Reranking` (and ElBruno.BM25)
- **License:** MIT (MUST be included at root)
- **Visibility:** Public
- **Code location:** `src/` folder (all C# code and tests)
- **Documentation location:** `docs/` folder at root (excludes README.md and LICENSE)
- **Readme & License:** At repository root

### Folder Structure (Template)
```
ElBruno.Reranking/
├── src/
│   ├── ElBruno.Reranking/
│   │   ├── IReranker.cs
│   │   ├── RerankResult.cs
│   │   ├── RerankOptions.cs
│   │   ├── Backends/
│   │   │   ├── BgeRerankModel.cs
│   │   │   ├── ClaudeReranker.cs
│   │   │   └── OllamaReranker.cs (v1.1)
│   │   └── ElBruno.Reranking.csproj
│   └── ElBruno.Reranking.Tests/
│       ├── BgeRerankTests.cs
│       ├── ClaudeRerankTests.cs
│       └── ElBruno.Reranking.Tests.csproj
├── docs/
│   ├── api/
│   │   └── [DocFX generated API reference]
│   ├── guides/
│   │   ├── quickstart.md
│   │   ├── onnx-backend.md
│   │   ├── claude-backend.md
│   │   ├── custom-reranker.md
│   │   └── performance-tuning.md
│   ├── benchmarks.md
│   ├── architecture.md
│   ├── cost-estimation.md
│   ├── roadmap.md
│   ├── assets/
│   │   ├── hero-reranking-blog.png
│   │   ├── linkedin-reranking-announcement.png
│   │   ├── twitter-reranking-announcement.png
│   │   └── github-social-preview.png
│   └── promotion/
│       ├── image-generation-prompts.md
│       ├── blog-post-reranking-announcement.md
│       ├── linkedin-post.md
│       └── twitter-post.md
├── README.md
├── LICENSE
├── .gitignore
├── .github/
│   └── workflows/
│       ├── build.yml
│       └── publish-nuget.yml
└── docfx.json
```

---

## NuGet Publish Process

### Reference Implementation
**Copy publishing workflow from:** `ElBruno.LocalLLMs` repository (`.github/workflows/publish-nuget.yml`)

**Key elements:**
- Trigger on git tag push (e.g., `v1.0.0`)
- Automatic version extraction from tag
- Build + test before publish
- Publish to NuGet.org using API key (GitHub Actions secret: `NUGET_API_KEY`)
- Create GitHub Release with changelog

**NuGet Package Metadata**
- **Package ID:** `ElBruno.Reranking` (or `ElBruno.BM25`)
- **License:** MIT (SPDX: `MIT`)
- **Repository URL:** `https://github.com/elbruno/ElBruno.Reranking`
- **Project URL:** Repository homepage
- **Tags:** Comma-separated, library-specific

---

## README Template

### Reference
**Template from:** `ElBruno.LocalLLMs` README

### Required Sections
1. **Title + Tagline**
   ```markdown
   # ElBruno.Reranking
   **Semantic reranking for .NET: Local-first ONNX, cloud-ready.**
   ```

2. **Badges** (GitHub, NuGet, License)
   ```markdown
   ![Build Status](https://github.com/elbruno/ElBruno.Reranking/workflows/build/badge.svg)
   [![NuGet Version](https://img.shields.io/nuget/v/ElBruno.Reranking.svg)](https://www.nuget.org/packages/ElBruno.Reranking)
   [![NuGet Downloads](https://img.shields.io/nuget/dt/ElBruno.Reranking.svg)](https://www.nuget.org/packages/ElBruno.Reranking)
   [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
   ```

3. **Features**
   - Bullet list of key capabilities

4. **Installation**
   ```bash
   dotnet add package ElBruno.Reranking
   ```

5. **Quick Start** (3-minute example)
   - Minimal code example
   - Output screenshot/results

6. **Documentation Link**
   - Link to `docs/api/`
   - Link to `docs/guides/`

7. **Performance Benchmarks**
   - Table of latency, throughput, model size

8. **Contributing**
   - How to contribute
   - Development setup

9. **About Author**
   ```markdown
   ## Author
   **ElBruno** — AI/ML engineer, open-source contributor
   - GitHub: https://github.com/elbruno
   - Twitter: [@elbruno](https://twitter.com/elbruno)
   ```

10. **License**
    - Link to LICENSE file

---

## Beta Mode: Promotion Assets

### When Libraries Reach Beta
Create and publish promotion content across web and social channels.

### Asset Locations
```
docs/
├── promotion/
│   ├── image-generation-prompts.md     ← All image generation instructions
│   ├── blog-post-announcement.md        ← Blog content
│   ├── linkedin-post.md                 ← LinkedIn copy
│   └── twitter-post.md                  ← Twitter/X copy
└── assets/
    ├── hero-blog.png                    ← Blog hero image
    ├── linkedin-announcement.png        ← LinkedIn post image
    ├── twitter-announcement.png         ← Twitter post image
    ├── github-social-preview.png        ← GitHub repo preview
    └── nuget-icon.png                   ← NuGet package icon (256x256)
```

### Blog Post (`docs/promotion/blog-post-announcement.md`)

**Template:**
```markdown
# [Library Name]: Introducing ElBruno.Reranking to .NET Developers

## Introduction
- Problem statement
- Why .NET needed this
- Why now?

## The Challenge
- Current state of reranking in .NET
- Missing tools/capabilities
- Impact on RAG systems

## The Solution: ElBruno.Reranking
- What it is
- Key features
- Why it's different

## Performance & Benchmarks
- Latency metrics
- Comparison (ONNX vs Claude)
- Real-world use case

## Getting Started
- Installation
- 3-minute quickstart
- Next steps

## Roadmap
- v1.1 features
- Community feedback opportunity

## Resources
- GitHub link
- Documentation
- NuGet package link
```

**Publication targets:**
- dev.to
- Medium
- Personal blog
- LinkedIn article

### LinkedIn Post (`docs/promotion/linkedin-post.md`)

**Template:**
```
🚀 Introducing ElBruno.Reranking — Semantic Search Ranking for .NET

Problem: How do you improve RAG precision without expensive API calls?

Solution: ElBruno.Reranking with local-first ONNX inference + optional Claude API

✨ Key Features:
🔹 BGE-Reranker-base (278M params, ONNX) — runs on CPU
🔹 Zero API keys required (local-first by default)
🔹 Optional Claude API for maximum precision
🔹 98%+ R@5 on LongMemEval benchmark
🔹 <100ms latency per rerank operation

Why it matters:
📈 Improves search relevance from 96% to 98%+ (R@5)
💰 No cloud costs by default (ONNX is free)
🔒 Keeps sensitive data on-device
⚡ Production-ready, thread-safe

Try it today:
```bash
dotnet add package ElBruno.Reranking
```

GitHub: [link]
Docs: [link]
NuGet: [link]

#dotnet #ai #semanticsearch #rag #opensource #machinelearning
```

### Twitter/X Post (`docs/promotion/twitter-post.md`)

**Template:**
```
🎯 ElBruno.Reranking is live on NuGet!

Semantic reranking for .NET — improve your RAG precision 📈

✨ 278M param ONNX model (local-first)
✨ <100ms per rerank
✨ Zero API keys required
✨ Production-ready

https://github.com/elbruno/ElBruno.Reranking
https://www.nuget.org/packages/ElBruno.Reranking

#dotnet #ai #opensource
```

**Variant for YouTube Shorts/TikTok:**
```
30-second hook:
"How to get 98%+ search accuracy in your .NET RAG without expensive APIs?
ElBruno.Reranking — runs locally, takes milliseconds, costs $0. Open source. Available now."

#dotnet #ai #tutorial #coding
```

---

## Image Generation Prompts

### Automated Image Generation via t2i Skill

**Process:** Use the t2i skill to automatically generate all promotion images from the prompts below:

```bash
# Example command (using Squad's t2i skill or similar)
squad skill t2i --prompt "Design a professional icon for ElBruno.Reranking..." --output docs/assets/reranking-icon.png --size 256x256
```

**t2i Skill Configuration:**
- **Skill location:** `.squad/skills/t2i/` or built-in t2i capability
- **Batch generation:** Process all prompts in `docs/promotion/image-generation-prompts.md` sequentially
- **Output path:** All images → `docs/assets/` (organize by size/use case if needed)
- **Quality check:** After generation, review images for brand consistency, readability, and quality. Re-generate if needed with refined prompts.
- **Fallback:** If automated generation fails or produces low-quality output, use manual generation tools (DALL-E, Midjourney, etc.)

**Tools that support t2i:**
- Squad CLI with t2i skill installed
- GitHub Copilot (built-in image generation in some contexts)
- DALL-E 3 / OpenAI API
- Midjourney
- Cursor IDE
- Other image generation APIs

### Document Location
**File:** `docs/promotion/image-generation-prompts.md`

**Generation instruction:** Feed each prompt below into your t2i tool. Save output to the specified file path in `docs/assets/`.

### NuGet Package Icon (256x256 PNG)

**Prompt for ElBruno.Reranking:**
```
Design a professional icon for ElBruno.Reranking (semantic reranking library for .NET).
Represent: ranking, precision, intelligence, neural networks.
Visual style: Modern, minimalist, tech-forward.
Include: Upward arrow or ranking bars (indicating reranking), 
brain/neural elements, or layered circles (representing ranking layers).
Color palette: Purples, blues, whites (distinct from BM25 to avoid confusion).
Square format, 256x256 pixels, transparent background.
Suitable for NuGet package branding.
```

**Prompt for ElBruno.BM25:**
```
Design a professional icon for ElBruno.BM25 (full-text search library for .NET).
Represent: searching, keyword matching, information retrieval.
Visual style: Modern, minimalist, tech-forward.
Include: Magnifying glass, search bars, or keyword/text ranking visualization.
Color palette: Blues, greens, whites (distinct from Reranking to avoid confusion).
Square format, 256x256 pixels, transparent background.
Suitable for NuGet package branding.
```

### Blog Post Hero Image

**Prompt for Reranking:**
```
Create a professional hero image for blog post: "ElBruno.Reranking: Semantic Search Precision for .NET"
Concepts to visualize: ranking optimization, neural networks, precise results, performance improvement.
Visual elements: AI/ML network visualization, upward trending graph, 
search results with precision scores, LLM integration.
Modern tech aesthetic: Clean layout, purple/blue color scheme, 
subtle gradients, professional typography.
Aspect ratio: 16:9 (1200x675 or 1600x900 pixels).
Include subtle .NET or C# branding without being too literal.
```

**Prompt for BM25:**
```
Create a professional hero image for blog post: "ElBruno.BM25: Lightweight Full-Text Search for .NET"
Concepts to visualize: fast searching, lightweight architecture, performance, information retrieval.
Visual elements: Minimalist search UI, performance metrics/graphs, 
code snippets (C#), search algorithm visualization.
Modern tech aesthetic: Clean layout, blue/green color scheme, 
subtle gradients, professional typography.
Aspect ratio: 16:9 (1200x675 or 1600x900 pixels).
Include subtle .NET or C# branding without being too literal.
```

### LinkedIn Post Image

**Prompt for Reranking:**
```
Create a professional LinkedIn announcement graphic for ElBruno.Reranking.
Include: Product name "ElBruno.Reranking", tagline "Semantic Reranking for .NET",
key metrics (98%+ R@5, <100ms latency, ONNX local-first).
Design: Corporate, modern, eye-catching. 
Color palette: Purples, blues, whites. Professional gradient background.
Typography: Bold title, clear subtitle, readable metrics.
Square format: 1080x1080 pixels or 16:9 for carousel.
Add subtle call-to-action: "Available on NuGet" or "GitHub"
```

**Prompt for BM25:**
```
Create a professional LinkedIn announcement graphic for ElBruno.BM25.
Include: Product name "ElBruno.BM25", tagline "Full-Text Search for .NET",
key metrics (1M docs <5s, <50ms search, zero dependencies).
Design: Corporate, modern, eye-catching.
Color palette: Blues, greens, whites. Professional gradient background.
Typography: Bold title, clear subtitle, readable metrics.
Square format: 1080x1080 pixels or 16:9 for carousel.
Add subtle call-to-action: "Available on NuGet" or "GitHub"
```

### Twitter/X Post Image

**Prompt for Reranking:**
```
Create a compact social media graphic for Twitter/X announcing ElBruno.Reranking.
Headline: "98%+ Search Precision. <100ms Latency. Zero API Costs."
Include: Product name, 2-3 key stats, NuGet badge or QR code.
Design: Vibrant, modern, highly visible on mobile. Dark background recommended.
Color palette: Contrasting purples/blues with white text, yellow accents.
Aspect ratio: 16:9 (1024x576 or 1200x675 pixels).
Make it scrollstop-worthy — use strong contrast and visual hierarchy.
```

**Prompt for BM25:**
```
Create a compact social media graphic for Twitter/X announcing ElBruno.BM25.
Headline: "1M Documents. 5 Seconds. Zero Dependencies."
Include: Product name, 2-3 key stats, NuGet badge or QR code.
Design: Vibrant, modern, highly visible on mobile. Dark background recommended.
Color palette: Contrasting blues/greens with white text, yellow accents.
Aspect ratio: 16:9 (1024x576 or 1200x675 pixels).
Make it scrollstop-worthy — use strong contrast and visual hierarchy.
```

### GitHub Repository Social Preview Image

**Prompt (General Template):**
```
Create a GitHub social preview image for ElBruno.[LibraryName] repository.
This image appears when sharing the repo link on social media.

For BM25:
Include: Repository name "ElBruno.BM25", description "Full-Text Search Library",
key tagline "Fast | Lightweight | .NET".

For Reranking:
Include: Repository name "ElBruno.Reranking", description "Semantic Reranking Library",
key tagline "Precise | Local-First | .NET".

Design: Professional GitHub-style branding.
Colors: GitHub-compatible (dark background, bright accent colors matching library theme).
Aspect ratio: 16:9 (1280x640 pixels).
Include subtle code/search visual metaphor (but don't overcomplicate).
Typography: Bold, readable, hierarchical.
Ensure library name is immediately visible at 200x200 pixel preview size.
```

### Icon Files to Generate

**Files to Create:**
```
docs/assets/
├── bm25-icon.png                    ← 256x256 for NuGet
├── reranking-icon.png               ← 256x256 for NuGet
├── hero-bm25-blog.png               ← 1200x675 (16:9)
├── hero-reranking-blog.png          ← 1200x675 (16:9)
├── linkedin-bm25.png                ← 1080x1080 (square)
├── linkedin-reranking.png           ← 1080x1080 (square)
├── twitter-bm25.png                 ← 1024x576 (16:9)
├── twitter-reranking.png            ← 1024x576 (16:9)
├── github-social-bm25.png           ← 1280x640 (16:9)
└── github-social-reranking.png      ← 1280x640 (16:9)
```

**Reference for icon in .csproj:**
```xml
<PropertyGroup>
  <PackageIcon>nuget-icon.png</PackageIcon>
  <PackageIconUrl>https://raw.githubusercontent.com/elbruno/ElBruno.Reranking/main/docs/assets/reranking-icon.png</PackageIconUrl>
</PropertyGroup>

<ItemGroup>
  <None Include="docs/assets/reranking-icon.png" Pack="true" PackagePath="\" />
</ItemGroup>
```

---

## Beta to Release Checklist

### Pre-Release (Beta)
- [ ] Core functionality complete and tested
- [ ] Performance benchmarks finalized
- [ ] Documentation complete
- [ ] Examples all working
- [ ] NuGet package structure ready

### Beta Release
- [ ] Create NuGet pre-release package (v1.0.0-beta.1)
- [ ] Publish to NuGet.org (pre-release channel)
- [ ] Create GitHub release (mark as pre-release)
- [ ] Announce on GitHub Discussions

### Promotion (Beta → Release)
- [ ] Image generation prompts finalized
- [ ] All promotional images created (9 total)
- [ ] Blog post written and scheduled
- [ ] LinkedIn post drafted
- [ ] Twitter post drafted

### Release Day
- [ ] Bump version to v1.0.0
- [ ] Update CHANGELOG.md
- [ ] Publish NuGet package (production)
- [ ] Create GitHub Release (production)
- [ ] Publish blog post
- [ ] Post LinkedIn article
- [ ] Tweet announcement
- [ ] Update README with badges

### Post-Release (Week 1)
- [ ] Gather feedback
- [ ] Monitor NuGet downloads
- [ ] Update GitHub stars tracking
- [ ] Plan v1.1 features based on feedback

---

## Summary

Both libraries follow a **consistent structure and promotion workflow:**

1. **Repository:** Public, MIT license, `src/` + `docs/` split
2. **Publishing:** Leverage ElBruno.LocalLLMs publish workflow
3. **README:** Follow ElBruno.LocalLLMs template (badges, author, structure)
4. **Documentation:** Comprehensive docs/ folder with guides, API, benchmarks
5. **Promotion:** Blog, LinkedIn, Twitter content + professional images
6. **Brand Consistency:** Distinct icons and color schemes for each library
7. **Timeline:** Beta → blog + social → v1.0 release

---

**END OF ADDENDUM**
