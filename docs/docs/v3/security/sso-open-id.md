# Single Sign-on with OpenID Connect

Single Sign-on with OpenID Connect lets you use existing credentials configured in a [UAA Server](https://github.com/cloudfoundry/uaa) or [TAS Single-Sign-on service](https://docs.pivotal.io/p-identity) for authentication and authorization in ASP.NET Core applications.

## Usage

Steeltoe builds on top of `Microsoft.AspNetCore.Authentication.OpenIdConnect`. You may benefit from reading more about using [OpenID Connect in ASP.NET Core](https://andrewlock.net/an-introduction-to-openid-connect-in-asp-net-core/).

Usage of Steeltoe's OpenID Connect provider is effectively identical to that of the OAuth2 provider, although the behind-the-scenes story is a little different. The OpenID Connect provider uses Microsoft's OpenId Connect implementation, and settings are based on [`Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions`](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.openidconnect.openidconnectoptions), with these additional properties:

| Name | Description | Default |
| --- | --- | --- |
| `AdditionalScopes` | Scopes to request for tokens in addition to `openid`. | `string.Empty` |
| `Timeout` | The timeout (in milliseconds) for calls to the auth server. | 100000 |
| `ValidateCertificates` | Validate Auth server certificate. | `true` |

>Each setting above must be prefixed with `Security:Oauth2:Client`.

Aside from the different base class for options, the only usage change is to call `.AddCloudFoundryOpenId` instead of `.AddCloudFoundryOAuth`.
