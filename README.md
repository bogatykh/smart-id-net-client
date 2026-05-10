<div align="center">
  <img src="assets/nuget/package-icon.png" alt="Smart-ID" width="64" height="64">
</div>

# Smart-ID .NET client

[![Build and Test](https://github.com/bogatykh/smart-id-net-client/actions/workflows/dotnet.yml/badge.svg)](https://github.com/bogatykh/smart-id-net-client/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

.NET **Standard 2.0** client for [Smart-ID](https://www.smart-id.com) **RP API v3** (device-link and notification flows). Ported from the official [smart-id-java-client](https://github.com/SK-EID/smart-id-java-client); public surface and behaviour follow the Java library where applicable.

Official protocol documentation: [Smart-ID RP API](https://sk-eid.github.io/smart-id-documentation/).

## Table of contents

- [Introduction](#introduction)
- [Features](#features)
- [Requirements](#requirements)
- [Getting the library](#getting-the-library)
- [How to use (API v3)](#how-to-use-api-v3)
  - [Test accounts](#test-accounts-for-testing)
  - [Logging HTTP traffic](#logging-http-traffic)
  - [Configuring `SmartIdClient`](#configuring-smartidclient)
  - [TLS to Smart-ID](#tls-to-smart-id)
  - [Device-link flows](#device-link-flows)
    - [QR codes (third-party libraries)](#qr-codes-third-party-libraries)
  - [Notification-based flows](#notification-based-flows)
  - [Linked notification signature](#linked-notification-signature)
  - [Certificate by document number](#certificate-by-document-number)
  - [Session status](#session-status)
  - [Validating session results](#validating-session-results)
  - [Callback URL helpers](#callback-url-helpers)
  - [Exceptions](#exceptions)
- [Testing this repository](#testing-this-repository)
- [License](#license)

## Introduction

Use this library to integrate Smart-ID authentication, qualified signing, and certificate discovery into .NET applications. All session initiation and status calls are **asynchronous** (`async` / `Task`).

## Features

- User authentication (device-link and notification)
- Digital signature sessions (device-link and notification)
- Device-link certificate choice and **linked** notification signature flow
- Query end-user signing certificate by document number
- Device-link URI construction (`DeviceLinkBuilder`) with `authCode`
- Session status polling and response validation (PKIX-style trust for user certificates)

## Requirements

- **.NET** implementing .NET Standard 2.0 (e.g. .NET Framework 4.6.1+, .NET Core 2.0+, modern .NET)
- Runtime dependencies (pulled in with the package): **BouncyCastle.Cryptography**, **System.Text.Json**

## Getting the library

Add a project reference to `src/SmartId/SmartId.csproj` (or pack the project and reference the resulting NuGet).

```xml
<ItemGroup>
  <ProjectReference Include="path\to\SmartId.csproj" />
</ItemGroup>
```

Main namespace: **`SK.SmartId`**. REST connector types: **`SK.SmartId.Rest`**. DTOs: **`SK.SmartId.Rest.Dao`**.

## How to use (API v3)

Import types from `SK.SmartId` and `SK.SmartId.Rest` as needed. The entry point is **`SmartIdClient`**, which exposes factory methods for request builders (e.g. `CreateDeviceLinkAuthentication()`, `CreateNotificationSignature()`) and **`CreateDynamicContent()`** for **`DeviceLinkBuilder`**.

### Test accounts for testing

Use Smart-ID demo accounts, semantics identifiers, and document numbers from SK documentation:  
[Test accounts](https://sk-eid.github.io/smart-id-documentation/test_accounts.html).

**Public demo RP (used in examples below, integration tests, and the official Java README):**

| Setting | Value |
|--------|--------|
| Relying party UUID | `00000000-0000-4000-8000-000000000000` |
| Relying party name | `DEMO` |
| API base URL | `https://sid.demo.sk.ee/smart-id-rp/v3/` |
| Device-link scheme (demo) | `smart-id-demo` (instead of default `smart-id`) |

Use your own registered RP UUID and name in production. Smart-ID Basic (ADVANCED) accounts are not supported on DEMO — see SK environment docs.

### Logging HTTP traffic

There is no global logging switch. To log requests and responses (similar in spirit to the Java client’s Jersey filter), build an **`HttpClient`** whose handler chain includes **`SmartIdHttpLoggingHandler`** (`SK.SmartId.Rest`), then pass that client to **`SmartIdClient.SetConfiguredClient(HttpClient)`**.

Assign **`LogDebug`** / **`LogTrace`** delegates to write method + URI, status lines, and optional bodies.

### Configuring `SmartIdClient`

```csharp
var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
// Optional: wrap handler with SmartIdHttpLoggingHandler for diagnostics.

var client = new SmartIdClient();
client.SetRelyingPartyUUID("00000000-0000-4000-8000-000000000000");
client.SetRelyingPartyName("DEMO");
client.SetHostUrl("https://sid.demo.sk.ee/smart-id-rp/v3/");
client.SetConfiguredClient(httpClient);
```

Optional:

- **`SetPollingSleepTimeout(TimeSpan)`** — delay between session status polls (default 1 second).
- **`SetSessionStatusResponseSocketOpenTime(TimeSpan?)`** — forwarded to the REST connector for long-poll style `timeoutMs` on session status GET (when supported by your connector implementation).

You can replace the connector entirely with **`SmartIdClient.SmartIdConnector = myConnector`** if you implement **`ISmartIdConnector`**.

### TLS to Smart-ID

Unlike the Java client’s **`setTrustStore(JKS)`**, this library does not ship a JKS loader. Configure TLS the usual .NET way:

- Use the default **`HttpClientHandler`** certificate validation (OS trust store), or
- Provide a custom **`HttpClientHandler`** / **`SocketsHttpHandler`** with **`ServerCertificateCustomValidationCallback`** or client certificates as required by your environment.

Pin or load SK demo/production TLS certificates per [Smart-ID HTTPS documentation](https://sk-eid.github.io/smart-id-documentation/https_pinning.html).

**Certificate chain validation for session responses** (user TLS auth certs) is separate: use **`CertificateValidatorImpl`** with embedded PEM trust material (**`CertificateValidatorImpl.CreateDefault()`**) or construct it with your own anchor/intermediate **`X509Certificate2`** list. This mirrors validating the authentication/signing certificate from `SessionStatus`, not the TLS channel.

### Device-link flows

Device-link sessions are started via **`SmartIdClient.CreateDeviceLinkAuthentication()`**, **`CreateDeviceLinkSignature()`**, or **`CreateDeviceLinkCertificateRequest()`**. Typical pattern:

1. Configure the builder (`WithRpChallenge`, `WithSemanticsIdentifier` or `WithDocumentNumber`, `WithInteractions`, etc.).
2. Call **`InitAsync(CancellationToken)`** on the builder to obtain **`DeviceLinkSessionResponse`**.
3. Persist **`GetAuthenticationSessionRequest()`** (or the equivalent request object) for later validation.
4. Build the user-facing link with **`client.CreateDynamicContent()`** → **`DeviceLinkBuilder`**: set scheme (e.g. **`smart-id-demo`** for demo), **`WithDeviceLinkBase`**, **`WithDeviceLinkType`**, **`WithSessionType`**, **`WithSessionToken`**, **`WithDigest`**, **`WithLang`**, **`WithElapsedSeconds`** (required for QR), **`WithInteractions`** where applicable, then **`BuildDeviceLink(sessionSecret)`**.

Parameter semantics match the Java client and [device link flows](https://sk-eid.github.io/smart-id-documentation/rp-api/device_link_flows.html).

#### QR codes (third-party libraries)

The Java client includes a helper that builds QR images (ZXing). **This .NET library does not ship QR encoding**: adding it would drag in **heavy optional dependencies** (image stacks, barcode bindings, security surface) for every consumer, while many backends only forward the device-link string to a SPA or mobile app that already renders QR.

**Use a third-party library** where you need a bitmap or data URI, for example **QRCoder**, **ZXing.Net**, or platform APIs. Encode the **full device-link URI** returned from `DeviceLinkBuilder.BuildDeviceLink(sessionSecret)` (same string you would pass to the Java `QrCodeGenerator`).

### Notification-based flows

Use **`CreateNotificationAuthentication()`**, **`CreateNotificationSignature()`**, or **`CreateNotificationCertificateChoice()`**. Call **`InitAsync`** on the corresponding builder to start a session; poll session status the same way as for device-link flows.

### Linked notification signature

After a device-link **certificate choice** session completes, you can start a **`CreateLinkedNotificationSignature()`** session with **`WithLinkedSessionID`**, **`WithDocumentNumber`**, digest input (**`SignableData`** / **`SignableHash`**), algorithm, and interactions. Initiate with **`InitAsync`**.

### Certificate by document number

```csharp
var certResult = await client.CreateCertificateByDocumentNumber()
    .WithDocumentNumber("PNOEE-…")
    .GetCertificateByDocumentNumberAsync();
```

### Session status

- One-shot: **`await client.SmartIdConnector.GetSessionStatusAsync(sessionId, cancellationToken)`**
- Poll until **`State`** is **`COMPLETE`**: **`await client.GetSessionStatusPoller().FetchFinalSessionStatusAsync(sessionId, cancellationToken)`**

### Validating session results

Use the typed validators (same roles as in the Java client), for example:

- **`DeviceLinkAuthenticationResponseValidator`** — device-link authentication; use **`DefaultSetupWithCertificateValidator(ICertificateValidator)`** or inject dependencies as in the Java **`defaultSetupWithCertificateValidator`**.
- **`NotificationAuthenticationResponseValidator`**
- **`CertificateChoiceResponseValidator`**
- **`SignatureResponseValidator`**

Pass **`SessionStatus`**, the original session request DTO, challenge/certificate level as required by the flow. **`CertificateValidatorImpl.CreateDefault()`** supplies SK test/production CA material bundled as PEM resources for PKIX-style trust (**BouncyCastle** path building).

### Callback URL helpers

For Web2App / App2App flows, **`SK.SmartId.Util.CallbackUrlUtil`** provides **`CreateCallbackUrl`**, **`ValidateSessionSecretDigest`**, and related helpers aligned with the Java client.

### Exceptions

Typed errors live under **`SK.SmartId.Exceptions`**, **`SK.SmartId.Exceptions.Permanent`**, **`SK.SmartId.Exceptions.UserActions`**, **`SK.SmartId.Exceptions.UserAccounts`**, following the Java exception mapping (e.g. user refused, timeout, document unusable).

## Testing this repository

```bash
dotnet test smart-id-net-client.sln --filter "Category!=DemoIntegrationReadme"
```

Integration tests tagged **`DemoIntegrationReadme`** (live demo + mock device-link) are excluded from the default command above; see [`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml). Java ↔ .NET test mapping: [`docs/java-unit-test-parity.md`](docs/java-unit-test-parity.md).

## License

See [LICENSE](LICENSE). The library is published under the **MIT License**, matching the [upstream smart-id-java-client](https://github.com/SK-EID/smart-id-java-client) license; copyright notices for both the original work (SK ID Solutions AS) and this port are in `LICENSE`.

The Smart-ID graphic in this README and in the NuGet package is taken from the [official Smart-ID documentation](https://sk-eid.github.io/smart-id-documentation/logos.html) (Smart-ID login assets; stated there as free to use). It is resized for NuGet; *Smart-ID* is a trademark of its owner. This project is not affiliated with SK ID Solutions.
