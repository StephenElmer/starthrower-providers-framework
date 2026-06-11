# StarThrower.WcfProviders

ASP.NET `MembershipProvider`, `RoleProvider`, and `ProfileProvider` implementations that
delegate to remote WCF/REST services defined by
[`StarThrower.WcfProviders.Contract`](../StarThrower.WcfProviders.Contract/README.md).

> **Not a NuGet package.** This project targets `net48` and depends on `System.ServiceModel`
> (WCF), `System.ServiceModel.Web`, and the ASP.NET Framework provider model
> (`MembershipProvider`/`RoleProvider`/`ProfileProvider` from `System.Web.Security` /
> `System.Web.Profile`), none of which exist on modern .NET. WCF itself would require
> migration to `CoreWCF`. It is excluded from the net10.0 migration and NuGet packaging (see
> the project's `CLAUDE.md` for the full rationale) and has no `IsPackable`/`PackageReadme`
> metadata. This README exists for documentation purposes only.

These providers are the client-side counterpart to a service that implements
`IMembershipService`, `IRoleService`, and `IProfileService` — for example, services backed by
[`StarThrower.EfProviders`](../StarThrower.EfProviders/README.md) — letting an ASP.NET
application use Membership/Role/Profile against a remote database server without a direct
database connection.

---

## Core Types

| Type | Description |
|---|---|
| `WcfMembershipProvider` | `MembershipProvider` implementation. Every member delegates to `ServiceWrapper.Instance`, which calls the remote `IMembershipService`. Adds `AdministrativePasswordReset(string userName)` beyond the standard `MembershipProvider` API. |
| `WcfRoleProvider` | `RoleProvider` implementation. Every member delegates to `ServiceWrapper.Instance`, which calls the remote `IRoleService`. |
| `WcfProfileProvider` | `ProfileProvider` implementation. Translates `SettingsPropertyCollection`/`SettingsPropertyValueCollection` to/from `Collection<ProfileItem>` (`TextProfileItem`/`BinaryProfileItem`) and delegates to `ServiceWrapper.Instance`, which calls the remote `IProfileService`. |
| `ServiceWrapper` | Singleton (`ServiceWrapper.Instance`) that owns the WCF channel creation and provides one method/property per contract operation across all three services. Reads `serviceUserName`, `servicePassword`, and `ignoreCertificateValidation` from `<appSettings>` on first access. |

---

## Configuration

`ServiceWrapper` opens a `WebChannelFactory<T>` per call using three named client endpoints —
`"membershipService"`, `"roleService"`, and `"profileService"` — which must be configured in
`<system.serviceModel><client>`, pointing at a host that implements the corresponding
`StarThrower.WcfProviders.Contract` interface:

```xml
<system.serviceModel>
  <bindings>
    <webHttpBinding>
      <binding name="public" maxReceivedMessageSize="2147483647">
        <readerQuotas maxArrayLength="2147483647" maxStringContentLength="2147483647"/>
        <security mode="None">
          <transport clientCredentialType="None"/>
        </security>
      </binding>
    </webHttpBinding>
  </bindings>
  <behaviors>
    <endpointBehaviors>
      <behavior name="web">
        <webHttp/>
      </behavior>
    </endpointBehaviors>
  </behaviors>
  <client>
    <endpoint name="membershipService" address="https://example.com/MembershipService/rest"
              binding="webHttpBinding" bindingConfiguration="public"
              contract="StarThrower.WcfProviders.Contract.IMembershipService"
              behaviorConfiguration="web"/>
    <endpoint name="roleService" address="https://example.com/RoleService/rest"
              binding="webHttpBinding" bindingConfiguration="public"
              contract="StarThrower.WcfProviders.Contract.IRoleService"
              behaviorConfiguration="web"/>
    <endpoint name="profileService" address="https://example.com/ProfileService/rest"
              binding="webHttpBinding" bindingConfiguration="public"
              contract="StarThrower.WcfProviders.Contract.IProfileService"
              behaviorConfiguration="web"/>
  </client>
</system.serviceModel>
```

`<appSettings>` controls `ServiceWrapper`'s behavior:

```xml
<appSettings>
  <add key="serviceUserName" value="..."/>
  <add key="servicePassword" value="..."/>
  <add key="ignoreCertificateValidation" value="false"/>
</appSettings>
```

Each provider is registered like its built-in `Sql*Provider` counterpart, e.g.:

```xml
<membership defaultProvider="WcfMembershipProvider">
  <providers>
    <add name="WcfMembershipProvider" type="StarThrower.WcfProviders.WcfMembershipProvider" applicationName="/" />
  </providers>
</membership>
```

---

## Usage Notes

- **Per-call channels.** `ServiceWrapper` creates a new `WebChannelFactory<T>`/channel on
  every call rather than caching one — each provider method opens, uses, and discards its
  own channel.
- **Credentials are conditional.** `serviceUserName`/`servicePassword` are only applied to
  the channel's `Credentials.UserName` if the endpoint's `WebHttpBinding.Security.Mode` is
  not `None`. With `security mode="None"` (as in the included `App.config` example), no
  credentials are sent.
- **`ignoreCertificateValidation`.** If `true`, `ServiceWrapper` registers a
  `ServicePointManager.ServerCertificateValidationCallback` that accepts all certificates —
  intended for self-signed certificates in development, not for production use.
- **Profile property serialization.** `WcfProfileProvider.GetPropertyValues` maps
  `SettingsSerializeAs.Binary` to `BinaryProfileItem` and `String`/`Xml` to `TextProfileItem`
  (any other `SerializeAs` throws `NotSupportedException`). `SetPropertyValues` branches on
  the *runtime type* of each property value: `String`, `Boolean`, `DateTime`, `Int32`, and
  `Guid` are sent as `TextProfileItem` (via `ToString()`); everything else is sent as
  `BinaryProfileItem` with a direct `byte[]` cast.
- **`App.config`** in this project is a development-time example and does not define the
  `membershipService`/`roleService`/`profileService` endpoints `ServiceWrapper` requires —
  a consuming application must supply its own `<system.serviceModel>` configuration as shown
  above.

---

## Dependencies

- [`StarThrower.WcfProviders.Contract`](../StarThrower.WcfProviders.Contract/README.md) — the `IMembershipService`/`IRoleService`/`IProfileService` contracts and `User`/`ProfileItem`/etc. data contracts that `ServiceWrapper` calls and translates to/from ASP.NET provider types.

It also depends on the .NET Framework assemblies `System.Configuration`, `System.ServiceModel`,
`System.ServiceModel.Web`, `System.Web`, and `System.Web.ApplicationServices`.

---

## License

Copyright © 2026 Stephen Elmer. Licensed under the [MIT License](../LICENSE.md).
