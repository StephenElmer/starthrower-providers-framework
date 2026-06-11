# StarThrower.Providers.Framework — Claude Code Context

This file provides context for Claude Code sessions working on the
StarThrower.Providers.Framework project. Read this before taking any action.

---

## Working Style

- **Explain before executing.** Describe what you are about to do and why before doing
  it. Wait for confirmation unless the task is explicitly scoped and pre-approved.
- **One step at a time.** Do not chain changes together autonomously.
- **Flag decisions.** Surface options rather than picking silently.
- **Ask before adding anything new.** New packages, new files, new patterns — ask first.
- **Do not run git commands.** Leave all source control operations to the developer.
- **Do not run dotnet test autonomously** unless explicitly asked.

---

## What This Repo Is

StarThrower.Providers.Framework is a set of legacy .NET Framework 4.8 provider libraries
extracted from the StarThrower.Utilities modernization project in June 2026.

These libraries were originally part of `StarThrower.Utilities` — a general-purpose C#
utility library dating to the early 2000s. They were separated because:

1. They depend on `System.Web` and WCF — both of which have no clean migration path to
   .NET 10 without a full architectural rewrite
2. They were causing complications in the modernized StarThrower.Utilities solution
   (net48/net10.0 mixed-TFM test runner friction with xUnit v3 MTP)
3. The `.Framework` suffix explicitly reserves the clean namespace
   (`StarThrower.EfProviders`, `StarThrower.WcfProviders`) for a future modern rebuild

### What These Libraries Were

Originally designed to eliminate boilerplate in web applications requiring a database
and user management — capturing the recurring patterns of MembershipProvider,
RoleProvider, ProfileProvider, and Entity Framework integration that the author found
himself rewriting on every new consulting project.

### Current Status

**Essentially frozen.** No active development is planned. These libraries are preserved
for reference and for any legacy applications still consuming them. The `.Framework`
namespace signals their status explicitly.

The eventual successor — a modern rebuild targeting .NET 10, ASP.NET Core Identity,
and EF Core — will live in a separate repo under the clean namespace
(`StarThrower.EfProviders`, `StarThrower.WcfProviders`), most likely as part of a
broader StarThrower.Utilities Phase 2 or a dedicated `starthrower-providers` repo.

---

## Projects

| Project | Namespace | Description |
|---|---|---|
| `StarThrower.EfProviders.Framework` | `StarThrower.EfProviders.Framework` | Entity Framework provider abstractions for .NET Framework |
| `StarThrower.WcfProviders.Framework` | `StarThrower.WcfProviders.Framework` | WCF service provider abstractions for .NET Framework |
| `StarThrower.WcfProviders.Contract.Framework` | `StarThrower.WcfProviders.Contract.Framework` | WCF contract definitions for .NET Framework |

### Test Projects

- `StarThrower.EfProviders.Framework.Test` — MSTest (VS Test), net48
- `StarThrower.WcfProviders.Framework.Test` — MSTest (VS Test), net48

### Other Projects

- `StarThrower.Providers.Framework.TestWebApp` — ASP.NET MVC 4 web app for provider
  testing. net48, not migrated, kept for reference only. **Builds successfully but
  cannot be run/debugged (F5) in Visual Studio 2026** — the SDK-style
  `Microsoft.NET.Sdk.Web` + net48 + classic `System.Web` combination isn't supported by
  VS's IIS Express debugging integration, which expects the legacy non-SDK web project
  format. Not worth fixing for a frozen repo.

---

## Repository Structure

```
Current/
    Code/                           ← Git repo root
        .gitignore
        CLAUDE.md                   ← this file
        PLAN.md
        README.md
        LICENSE.md
        Directory.Build.props
        StarThrower.public.snk
        StarThrower.Providers.Framework.sln
        StarThrower.EfProviders.Framework/
        StarThrower.EfProviders.Framework.Test/
        StarThrower.WcfProviders.Framework/
        StarThrower.WcfProviders.Framework.Test/
        StarThrower.WcfProviders.Contract.Framework/
        StarThrower.Providers.Framework.TestWebApp/
```

---

## Current State

- **Framework:** .NET Framework 4.8 throughout — no migration planned
- **Language:** C# (legacy, no modern idioms applied)
- **Source control:** Git / GitHub (extracted from StarThrower.Utilities repo)
- **Test framework:** MSTest (VS Test) — no migration planned
- **NuGet:** PackageReference style (migrated as part of StarThrower.Utilities Step 2a)
- **Namespace:** Renamed from original `StarThrower.EfProviders` /
  `StarThrower.WcfProviders` / `StarThrower.Providers.TestWebApp` to
  `StarThrower.EfProviders.Framework` / `StarThrower.WcfProviders.Framework` /
  `StarThrower.Providers.Framework.TestWebApp` to reserve clean namespaces for future
  rebuild
- **Status:** Frozen — no active development

---

## Key Technical Constraints

### System.Web Blocker
These projects depend on `System.Web` — the ASP.NET Framework provider model
(`MembershipProvider`, `RoleProvider`, `ProfileProvider`). `System.Web` does not exist
in .NET Core / .NET 5+. Migration to .NET 10 would require a complete architectural
rewrite using ASP.NET Core Identity — a separate project, not a migration.

### WCF
WCF server-side hosting does not exist in .NET Core. Migration requires `CoreWCF` — a
community-maintained port. This is a significant undertaking and out of scope here.

### Entity Framework
These libraries use EF6 Database-First approach. Migration to EF Core requires
scaffolding new DbContext/entity classes and rewriting ~3,500 lines of business logic.
Out of scope here.

---

## What NOT to Do

- Do not attempt to migrate these projects to net10.0 — that is a separate future effort
  in a separate repo
- Do not change public API surface — legacy applications may still consume these
- Do not add new features — this repo is frozen
- Do not run git commands
- Do not run dotnet test unless explicitly asked

---

## Relationship to Other StarThrower Projects

| Repo | Relationship |
|---|---|
| `starthrower-utilities` | Parent repo — these projects were extracted from here in June 2026 |
| `starthrower-providers` (future) | Planned modern rebuild of these libraries targeting .NET 10 / ASP.NET Core Identity / EF Core |
| `PROMPT` (separate repo, TFS) | May have previously referenced these libraries via project reference; to be evaluated during PROMPT modernization |
