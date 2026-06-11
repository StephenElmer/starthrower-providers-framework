# StarThrower.WcfProviders.Contract

WCF service and data contracts exposing the ASP.NET `MembershipProvider`, `RoleProvider`,
and `ProfileProvider` APIs as REST-style web services.

> **Not a NuGet package.** This project targets `net48` and depends on `System.ServiceModel`
> (WCF), `System.ServiceModel.Web`, and `System.Web`/`System.Web.Security`/`System.Web.Profile`
> types (`MembershipUser`, `MembershipCreateStatus`, `ProfileInfoCollection`,
> `ProfileAuthenticationOption`), none of which exist on modern .NET. WCF itself would require
> migration to `CoreWCF`. It is excluded from the net10.0 migration and NuGet packaging (see
> the project's `CLAUDE.md` for the full rationale) and has no `IsPackable`/`PackageReadme`
> metadata. This README exists for documentation purposes only.

---

## Service Contracts

Each service interface is a `[ServiceContract]` whose operations are `[OperationContract]`s
decorated with `[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]`,
exposing the service as a JSON/XML REST endpoint, and `[FaultContract(typeof(GenericFault))]`
for error responses.

| Interface | Mirrors | Description |
|---|---|---|
| `IMembershipService` | `MembershipProvider` | User CRUD, password management (change/reset/retrieve, including an extra `AdministrativePasswordReset`), lockout (`UnlockUser`), user lookup/search (`GetUserByName`, `GetUserByKey`, `FindUsersByEmail`, `FindUsersByName`, `GetAllUsers`), and provider configuration queries (`MinRequiredPasswordLength`, `PasswordFormat`, `RequiresUniqueEmail`, etc.). |
| `IRoleService` | `RoleProvider` | Role CRUD (`CreateRole`, `DeleteRole`, `RoleExists`), user-role assignment (`AddUsersToRoles`, `RemoveUsersFromRoles`), and membership queries (`GetRolesForUser`, `GetUsersInRole`, `FindUsersInRole`, `IsUserInRole`). |
| `IProfileService` | `ProfileProvider` | Profile property read/write (`GetPropertyValues`/`SetPropertyValues`), profile search (`FindProfilesByUserName`, `GetAllProfiles`, inactive-profile variants), and bulk deletion (`DeleteProfilesByUserName`, `DeleteProfilesByProfile`, `DeleteInactiveProfiles`). |

Each `UriTemplate` follows a `/lowercaseoperationname` convention (e.g.
`/changepassword`, `/getallroles`, `/findprofilesbyusername`).

---

## Data Contracts

| Type | Description |
|---|---|
| `User` | `[DataContract]` projection of `System.Web.Security.MembershipUser`. Provides a constructor that copies from a `MembershipUser` and `GetMembershipUser()` to convert back — used to carry membership user data over the wire without referencing `MembershipUser` as a wire type directly. |
| `CreateUserResult` | Wraps the `User` created by `IMembershipService.CreateUser` plus the resulting `MembershipCreateStatus`. |
| `FindUserResult` | A page of `User` results (`Collection<User>`) plus `TotalRecords`, returned by the `IMembershipService` find/search/list operations. |
| `GetProfilesResult` | Wraps a `ProfileInfoCollection` plus `TotalRecords`, returned by the `IProfileService` find/search/list operations. |
| `ProfileItem` | `[DataContract]` abstract base for a single `<profile>` property: `Name`, `PropertyType`, `DefaultValue`, and `SerializeAs` (`SettingsSerializeAs`). Declares `[KnownType(typeof(TextProfileItem))]` and `[KnownType(typeof(BinaryProfileItem))]` so the WCF serializer can handle either concrete type polymorphically. |
| `TextProfileItem` | `ProfileItem` subclass for string-serialized profile properties; adds `Value` (`string`). |
| `BinaryProfileItem` | `ProfileItem` subclass for binary-serialized profile properties; adds `Value` (`byte[]`). |
| `GenericFault` | `[DataContract]` fault payload (`Message`) used as the `[FaultContract]` type on every operation across all three service interfaces. |

---

## Usage Notes

- **REST + WCF dual exposure.** The `[WebInvoke]` attributes mean these contracts are
  intended to be hosted as JSON/XML REST endpoints (via `WebServiceHost` or a `webHttpBinding`
  endpoint), not just classic SOAP. The `[ServiceContract]`/`[OperationContract]` attributes
  alone would also support a SOAP binding if configured.
- **`User` vs. `MembershipUser`.** `MembershipUser` is not a WCF-serializable type with a
  parameterless constructor suitable for `[DataContract]` use as-is, so `User` exists purely
  to shuttle the same data across the wire; callers convert at the boundary with
  `new User(membershipUser)` and `user.GetMembershipUser()`.
- **`object ProviderUserKey` / `object DefaultValue`.** Both `User.ProviderUserKey` and
  `ProfileItem.DefaultValue` are typed `object`, matching the corresponding
  `MembershipProvider`/`ProfileProvider` APIs; the actual runtime type depends on the
  provider implementation (e.g. `StarThrower.EfProviders`) and the WCF data contract
  serializer's handling of `object` members.
- **Implementations.** `StarThrower.WcfProviders` implements these service contracts,
  typically delegating to an underlying `MembershipProvider`/`RoleProvider`/`ProfileProvider`
  such as those in `StarThrower.EfProviders`.

---

## Dependencies

This project has no references to other StarThrower packages. It depends on the .NET
Framework assemblies `System.Configuration`, `System.Runtime.Serialization`,
`System.ServiceModel`, `System.ServiceModel.Web`, `System.Web`, and
`System.Web.ApplicationServices`.

---

## License

Copyright © 2026 Stephen Elmer. Licensed under the [MIT License](../LICENSE.md).
