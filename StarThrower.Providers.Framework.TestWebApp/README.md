# StarThrower.Providers.TestWebApp

An ASP.NET MVC 4 sample web application demonstrating the
[`StarThrower.EfProviders`](../StarThrower.EfProviders/README.md) and
[`StarThrower.WcfProviders`](../StarThrower.WcfProviders/README.md) `MembershipProvider`,
`RoleProvider`, and `ProfileProvider` implementations in a real ASP.NET Forms-authentication
application.

> **Not a NuGet package.** This project targets `net48` and is an ASP.NET MVC 4 web
> application built on `System.Web`, `System.Web.Mvc`, and `System.Web.Security`'s Forms
> Authentication and provider model — none of which exist on ASP.NET Core / modern .NET.
> Migrating it would be a full rewrite as an ASP.NET Core application, not a migration, and
> is out of scope for Phase 1 (see the project's `CLAUDE.md`). It has no
> `IsPackable`/`PackageReadme` metadata. This README exists for documentation purposes only.

---

## What It Demonstrates

The app is a minimal MVC site — Home, Account (log on/off), and an `[Authorize]`-protected
Admin page — wired up to ASP.NET's standard `Membership`/`Roles`/`Profile` static APIs.
Its purpose is to exercise the StarThrower provider implementations end-to-end as a
consuming application would, rather than to be a useful application in its own right.

`Web.config` shows two complete, alternative provider configurations:

- **Active (uncommented):** `StarThrower.EfProviders.EfMembershipProvider`,
  `EfProfileProvider`, and `EfRoleProvider`, connecting directly to a SQL Server
  `aspnetdb`-schema database (`MembershipDB` connection string) — see
  [`StarThrower.EfProviders`](../StarThrower.EfProviders/README.md).
- **Commented-out alternative:** `StarThrower.WcfProviders.WcfMembershipProvider`,
  `WcfProfileProvider`, and `WcfRoleProvider`, plus the corresponding
  `<system.serviceModel>` `membershipService`/`roleService` client endpoints — see
  [`StarThrower.WcfProviders`](../StarThrower.WcfProviders/README.md). (A `profileService`
  endpoint would also be needed to use `WcfProfileProvider`; only `roleService` and
  `membershipService` are present.)

Switching between the two is a matter of commenting/uncommenting the corresponding
`<membership>`/`<profile>`/`<roleManager>` blocks (and `<system.serviceModel>` endpoints
for the WCF variant).

---

## Project Layout

| Area | Contents |
|---|---|
| `Controllers/` | `HomeController` (landing page), `AccountController` (log on/off via `IFormsAuthenticationService`/`IMembershipService`), `AdminController` (`[Authorize(Roles = "Administrator, User")]`-protected page). |
| `Common/` | `IMembershipService`/`AccountMembershipService` (thin wrapper over `Membership.Provider`), `IFormsAuthenticationService`/`FormsAuthenticationService` (wraps `FormsAuthentication.SetAuthCookie`/`SignOut`), `BaseController`/`BaseViewModel` (shared `IsLoggedIn` session flag). |
| `Models/` | `AccountLogOnViewModel`, `AdminViewModel`, `HomeViewModel` — all derive from `BaseViewModel`. |
| `Views/` | Razor views for Home, Account/LogOn, Admin, and the shared layout. |
| `App_Start/` | `RouteConfig`, `WebApiConfig`, `FilterConfig` — standard MVC 4 project template bootstrapping. |

---

## Running the App

This is an ASP.NET MVC 4 application; run it under IIS Express from Visual Studio (or
`dotnet build`/host with IIS). It requires:

- A SQL Server database matching the ASP.NET Membership/Profile/Role schema
  (`aspnet_*` tables, `vw_aspnet_*` views — see
  [`StarThrower.EfProviders`](../StarThrower.EfProviders/README.md#configuration)),
  reachable via the `MembershipDB` connection string in `Web.config`.
- If switching to the `StarThrower.WcfProviders` configuration: a running service host
  implementing `IMembershipService`/`IRoleService`/`IProfileService` reachable at the
  endpoints configured in `<system.serviceModel><client>`.

---

## Dependencies

- [`StarThrower.EfProviders`](../StarThrower.EfProviders/README.md) — the active `MembershipProvider`/`RoleProvider`/`ProfileProvider` implementations configured in `Web.config`.
- [`StarThrower.WcfProviders`](../StarThrower.WcfProviders/README.md) — the alternative remote-service-backed provider implementations shown (commented out) in `Web.config`.
- [`StarThrower.WcfProviders.Contract`](../StarThrower.WcfProviders.Contract/README.md) — service contracts referenced by the `<system.serviceModel>` client endpoints.

---

## License

Copyright © 2026 Stephen Elmer. Licensed under the [MIT License](../LICENSE.md).
