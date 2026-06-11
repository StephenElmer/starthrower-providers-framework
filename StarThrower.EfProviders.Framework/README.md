# StarThrower.EfProviders

Entity Framework 6 (Database-First) implementations of the ASP.NET `MembershipProvider`,
`RoleProvider`, and `ProfileProvider` against the standard ASP.NET Membership/Profile/Role
database schema (the `aspnet_*` tables and `vw_aspnet_*` views created by `aspnet_regsql`).

> **Not a NuGet package.** This project targets `net48` and depends on `System.Web`,
> `System.Data.Entity` (EF6), and the ASP.NET Framework provider model
> (`MembershipProvider`/`RoleProvider`/`ProfileProvider`), none of which exist on modern .NET.
> It is excluded from the net10.0 migration and NuGet packaging (see the project's
> `CLAUDE.md` for the full rationale) and has no `IsPackable`/`PackageReadme` metadata. This
> README exists for documentation purposes only.

---

## Core Types

| Type | Description |
|---|---|
| `EfMembershipProvider` | `MembershipProvider` implementation: user creation, password management (clear/encrypted/hashed), lockout, and user lookup/search, backed by `MembershipDB`. |
| `EfRoleProvider` | `RoleProvider` implementation: role CRUD, user-role assignment, and role membership queries, backed by `MembershipDB`. |
| `EfProfileProvider` | `ProfileProvider` implementation: reads/writes `<profile>` property values (string-serialized and binary), and supports profile search/deletion by inactivity, backed by `MembershipDB`. |
| `MembershipDB` | An EF6 `ObjectContext` (generated from `ProviderModel.edmx`, Database-First) exposing `ObjectSet<T>` properties for `aspnet_Applications`, `aspnet_Membership`, `aspnet_Paths`, `aspnet_PersonalizationAllUsers`, `aspnet_PersonalizationPerUser`, `aspnet_Profile`, `aspnet_Roles`, `aspnet_SchemaVersions`, `aspnet_Users`, `aspnet_WebEvent_Events`, and the corresponding `vw_aspnet_*` views. |
| `ProfileItem` *(internal)* | Describes one `<profile>` property (`Name`, `PropertyType`, `DefaultValue`, `SerializeAs`, `Storage`, `StartIndex`, `Length`) parsed from configuration. |
| `EfProviderUtil` *(internal)* | Shared provider helpers: config value parsing (`GetIntValue`, `GetBooleanValue`), parameter validation (`CheckParameter`, `CheckArrayParameter`), schema version checking, and `GetDefaultAppName`. |

All three providers follow the standard ASP.NET provider pattern: `Initialize(string name,
NameValueCollection config)` reads settings from the `<membership>`/`<roleManager>`/`<profile>`
configuration sections, and each data-access method opens a `using (MembershipDB db = new
MembershipDB())` context scoped to that call.

---

## Configuration

`MembershipDB`'s parameterless constructor resolves its connection via
`<connectionStrings>` using the name `MembershipDB`:

```xml
<connectionStrings>
  <add name="MembershipDB"
       connectionString="metadata=res://*/ProviderModel.csdl|res://*/ProviderModel.ssdl|res://*/ProviderModel.msl;
                          provider=System.Data.SqlClient;
                          provider connection string=&quot;data source=.;initial catalog=aspnetdb;integrated security=True;multipleactiveresultsets=True&quot;"
       providerName="System.Data.EntityClient" />
</connectionStrings>
```

Each provider is registered like its built-in `Sql*Provider` counterpart:

```xml
<membership defaultProvider="EfMembershipProvider">
  <providers>
    <add name="EfMembershipProvider"
         type="StarThrower.EfProviders.EfMembershipProvider"
         applicationName="/"
         enablePasswordRetrieval="false"
         enablePasswordReset="true"
         requiresQuestionAndAnswer="false"
         requiresUniqueEmail="true"
         maxInvalidPasswordAttempts="5"
         minRequiredPasswordLength="7"
         minRequiredNonalphanumericCharacters="1"
         passwordFormat="Hashed" />
  </providers>
</membership>

<roleManager defaultProvider="EfRoleProvider" enabled="true">
  <providers>
    <add name="EfRoleProvider" type="StarThrower.EfProviders.EfRoleProvider" applicationName="/" />
  </providers>
</roleManager>

<profile defaultProvider="EfProfileProvider">
  <providers>
    <add name="EfProfileProvider" type="StarThrower.EfProviders.EfProfileProvider" applicationName="/" />
  </providers>
  <properties>
    <add name="PreferredLanguage" type="System.String" />
  </properties>
</profile>
```

`applicationName` defaults to `EfProviderUtil.GetDefaultAppName()` (the ASP.NET application's
virtual path, or the host process name) if not specified, and must not exceed 256 characters.

---

## Usage Notes

- **`AdministrativePasswordReset(string userName)`** on `EfMembershipProvider` is an
  additional public method beyond the standard `MembershipProvider` API — it resets a user's
  password without requiring the password answer (unlike `ResetPassword`).
- **Password formats.** `passwordFormat` supports `Clear`, `Encrypted`, and `Hashed` (default).
  `Encrypted` relies on `MembershipProvider.EncryptPassword`/`DecryptPassword`, which require a
  `<machineKey>` configured in `web.config`.
- **Profile binary properties.** `EfProfileProvider` stores serialized profile properties in
  `aspnet_Profile.PropertyValuesBinary`/`PropertyValuesString`; binary properties are extracted
  with a private `ByteSubstring` helper (inlined from `StarThrower.ByteUtilities.ByteUtil
  .ByteSubstring` so this project has no project references — see `CLAUDE.md` for why).
- **Schema version check.** Each provider's data methods call
  `EfProviderUtil.CheckSchemaVersion`, which verifies `aspnet_SchemaVersions` contains a
  `Compatible` row for the feature/version it expects (`"1"` for membership/roles/profile),
  throwing `ProviderException` if the database schema doesn't match.

---

## Dependencies

This project has no references to other StarThrower packages. It depends on the .NET
Framework assemblies `System.Configuration`, `System.Data.Entity`, `System.Runtime.Serialization`,
`System.Security`, `System.Web`, and `System.Web.ApplicationServices`.

---

## License

Copyright © 2026 Stephen Elmer. Licensed under the [MIT License](../LICENSE.md).
