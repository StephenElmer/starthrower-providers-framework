# StarThrower.Providers.Framework

Legacy .NET Framework 4.8 provider libraries, extracted from
[StarThrower.Utilities](https://github.com/StephenElmer/starthrower-utilities) in June
2026.

## Status: Frozen

This repository is **not under active development**. It exists to preserve a set of
ASP.NET provider abstractions (MembershipProvider / RoleProvider / ProfileProvider) and
EF6 Database-First / WCF integration patterns that were originally written to eliminate
recurring boilerplate across consulting projects.

These libraries depend on `System.Web` and WCF server-side hosting, neither of which has
a clean migration path to modern .NET. As a result, they have been moved here — out of
the modernized `StarThrower.Utilities` solution — and renamed under the `.Framework`
suffix to:

- Explicitly signal their .NET Framework 4.8 origin and frozen status
- Reserve the clean `StarThrower.EfProviders` / `StarThrower.WcfProviders` namespaces
  for a future modern rebuild

## Projects

| Project | Description |
|---|---|
| `StarThrower.EfProviders.Framework` | EF6 Database-First provider abstractions |
| `StarThrower.EfProviders.Framework.Test` | MSTest tests for EF provider abstractions |
| `StarThrower.WcfProviders.Framework` | WCF service provider abstractions |
| `StarThrower.WcfProviders.Framework.Test` | MSTest tests for WCF provider abstractions |
| `StarThrower.WcfProviders.Contract.Framework` | WCF contract definitions |
| `StarThrower.Providers.Framework.TestWebApp` | ASP.NET MVC 4 web app used for provider testing |

All projects target **.NET Framework 4.8** and are not packaged for NuGet
(`IsPackable=false`).

## Migration Path

A modern rebuild targeting .NET 10, ASP.NET Core Identity, and EF Core is planned for a
future `starthrower-providers` repository under the clean (non-`.Framework`) namespace.
That work is a full architectural rewrite, not an in-place migration of this code.

## Related Projects

- [starthrower-utilities](https://github.com/StephenElmer/starthrower-utilities) —
  parent project; the modernized .NET 10 utility library these projects were extracted
  from.

## License

MIT — see [LICENSE.md](LICENSE.md).
