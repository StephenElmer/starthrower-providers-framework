# Context Briefing — StarThrower.Providers.Framework

**For use as an opening document in a new conversation.**

---

## Background

This work is part of a larger modernization project. The parent project is
**StarThrower.Utilities** — a general-purpose C# utility library dating to the early
2000s (originally VB6), modernized to .NET 10 / C# 14 in 2026 and published to GitHub
and NuGet as open source. Full details of that project are in a separate conversation.

## What Needs to Be Done

Three legacy .NET Framework 4.8 provider libraries are being extracted from the
`StarThrower.Utilities` solution and moved into their own separate GitHub repo:
`starthrower-providers-framework`.

### The Three Projects Being Extracted

| Original Name | New Name | Description |
|---|---|---|
| `StarThrower.EfProviders` | `StarThrower.EfProviders.Framework` | EF6 Database-First provider abstractions |
| `StarThrower.WcfProviders` | `StarThrower.WcfProviders.Framework` | WCF service provider abstractions |
| `StarThrower.WcfProviders.Contract` | `StarThrower.WcfProviders.Contract.Framework` | WCF contract definitions |

Plus their test projects and `StarThrower.Providers.TestWebApp` (ASP.NET MVC 4),
renamed to `StarThrower.Providers.Framework.TestWebApp`.

### Why They Are Being Extracted

1. They depend on `System.Web` and WCF — no clean migration path to .NET 10
2. They were causing friction in the modernized Utilities solution (net48/net10.0
   mixed-TFM test runner issues with xUnit v3 MTP mode)
3. Extracting them reserves the clean namespace (`StarThrower.EfProviders`,
   `StarThrower.WcfProviders`) for a future modern rebuild

### Why the Namespace Is Changing

The `.Framework` suffix is added to all namespaces (e.g. `StarThrower.EfProviders`
becomes `StarThrower.EfProviders.Framework`) to:
- Explicitly signal these are .NET Framework era libraries
- Reserve the clean namespace for a future .NET 10 / ASP.NET Core Identity / EF Core
  rebuild in a separate `starthrower-providers` repo
- Use `.Framework` specifically because that is the .NET ecosystem's own vocabulary for
  this distinction (as opposed to `.Legacy`, `.Classic`, `.Net48`, etc.)

---

## Tasks for This Conversation

### Step 1 — Create the new GitHub repo ✅
- Repo name: `starthrower-providers-framework`
- Public, blank (no README/gitignore initialized on GitHub)

### Step 2 — Set up local folder structure ✅
Following the same convention as StarThrower.Utilities:
```
D:\StarThrower\Development\StarThrower.Providers.Framework\Current\Code\
```

### Step 3 — Copy projects from Utilities ✅
Copy the following folders from:
`D:\StarThrower\Development\StarThrower.Utilities\Current\Code\`

Into the new location:
- `StarThrower.EfProviders\`
- `StarThrower.EfProviders.Test\`
- `StarThrower.WcfProviders\`
- `StarThrower.WcfProviders.Test\`
- `StarThrower.WcfProviders.Contract\`
- `StarThrower.Providers.TestWebApp\`

### Step 4 — Rename namespaces ✅
Find-and-replace across all `.cs`, `.csproj`, and `.sln` files:

| Find | Replace |
|---|---|
| `StarThrower.EfProviders` | `StarThrower.EfProviders.Framework` |
| `StarThrower.WcfProviders.Contract` | `StarThrower.WcfProviders.Contract.Framework` |
| `StarThrower.WcfProviders` | `StarThrower.WcfProviders.Framework` |
| `StarThrower.Providers.TestWebApp` | `StarThrower.Providers.Framework.TestWebApp` |

**Important:** Replace `WcfProviders.Contract` before `WcfProviders` to avoid
double-substitution. Verify no occurrences of the old namespace remain after the sweep.

Also rename directories and files to match (e.g. `StarThrower.EfProviders\` →
`StarThrower.EfProviders.Framework\`, `StarThrower.EfProviders.csproj` →
`StarThrower.EfProviders.Framework.csproj`, `StarThrower.Providers.TestWebApp\` →
`StarThrower.Providers.Framework.TestWebApp\`, etc.) — **done**.

### Step 5 — Create solution file ✅
Create `StarThrower.Providers.Framework.sln` referencing all six projects.

### Step 6 — Create supporting files ✅
- `.gitignore` — same as StarThrower.Utilities (Visual Studio template) — done
- `CLAUDE.md` — done, saved to repo root
- `README.md` — done, saved to repo root
- `Directory.Build.props` — minimal version for net48 projects — done
- `LICENSE.md` — done, copied from StarThrower.Utilities

### Step 7 — Verify build ✅
```powershell
dotnet build StarThrower.Providers.Framework.sln
```
Tests may or may not pass depending on database/WCF dependencies — build clean is
the minimum bar.

Build succeeded (0 warnings, 0 errors). Required adding `StarThrower.public.snk`
(copied from `starthrower-utilities` repo root) to the new repo root — three projects
reference `..\StarThrower.public.snk` as their `AssemblyOriginatorKeyFile`.

### Step 8 — Remove projects from StarThrower.Utilities.sln
Once the new repo is confirmed building, remove the six projects from
`StarThrower.Utilities.sln` and clean up CLAUDE.md in the Utilities repo.

### Step 9 — Git init and push
```powershell
git init
git add .
git commit -m "Initial commit — StarThrower.Providers.Framework extracted from StarThrower.Utilities, namespace renamed to .Framework"
git remote add origin https://github.com/StephenElmer/starthrower-providers-framework.git
git branch -M main
git push -u origin main
```

---

## Files Available

The following files were generated in the StarThrower.Utilities conversation and
apply (with minor adjustments) to this repo:

- **CLAUDE.md** — done, saved to repo root
- **Directory.Build.props** — done, saved to repo root
- **LICENSE.md** — done, copied from StarThrower.Utilities
- **README.md** — done, saved to repo root

---

## Notes on Directory.Build.props for net48

The Utilities `Directory.Build.props` uses several net10.0-specific conditions. The
Framework repo needs a simpler version — primarily just shared identity metadata:

```xml
<Project>
  <PropertyGroup>
    <Authors>Stephen Elmer</Authors>
    <Company>StarThrower Software</Company>
    <Copyright>Copyright © 2026 Stephen Elmer</Copyright>
    <NeutralLanguage>en-US</NeutralLanguage>
    <!-- These projects are NOT published to NuGet -->
    <IsPackable>false</IsPackable>
  </PropertyGroup>
</Project>
```

`IsPackable>false</IsPackable>` on all projects — these are not being published to
NuGet. They are preserved for reference only.

---

## Notes on README.md

The README should clearly communicate:
- What these libraries were (EF6/WCF/ASP.NET Framework provider boilerplate)
- Why they exist as a separate repo (extracted from StarThrower.Utilities)
- Their current status (frozen, no active development)
- The migration path (future `starthrower-providers` repo for the modern rebuild)
- Link back to `starthrower-utilities` as the parent project

---

## Developer Context

**Developer:** Stephen Elmer, senior C#/.NET consultant, StarThrower Software
**GitHub:** github.com/StephenElmer (investigating recovery of github.com/starthrower)
**Project:** Part of a career portfolio modernization supporting a transition toward
AI, data science, and civic tech roles
**Related repos:**
- `starthrower-utilities` — main modernized library (net10.0, NuGet packages)
- PROMPT (TFS, not yet migrated) — risk-scoring engine, future consumer of
  starthrower-utilities NuGet packages
