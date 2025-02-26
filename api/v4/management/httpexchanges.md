# HTTP Exchanges

The Steeltoe HTTP Exchanges endpoint provides the ability to view the last several requests made of your application.

When you activate this endpoint, an `IHttpExchangesRepository` implementation is registered that stores HTTP request/response information in memory, that can be retrieved by using the endpoint.

## Configure Settings

The following table describes the configuration settings that you can apply to the endpoint.
Each key must be prefixed with `Management:Endpoints:HttpExchanges:`.

| Key | Description | Default |
| --- | --- | --- |
| `Enabled` | Whether the endpoint is enabled. | `true` |
| `ID` | The unique ID of the endpoint. | `httpexchanges` |
| `Path` | The relative path at which the endpoint is exposed. | same as `ID` |
| `RequiredPermissions` | Permissions required to access the endpoint, when running on Cloud Foundry. | `Restricted` |
| `AllowedVerbs` | An array of HTTP verbs the endpoint is exposed at. | `GET` |
| `Capacity` | Size of the circular buffer of exchanges. | 100 |
| `IncludeRequestHeaders` | Whether to return headers from the HTTP request. | `true` |
| `RequestHeaders` | An array of HTTP request headers to return unredacted, in addition to the default set. | |
| `IncludeResponseHeaders` | Whether to return headers from the HTTP response. | `true` |
| `ResponseHeaders` | An array of HTTP response headers to return unredacted, in addition to the default set. | |
| `IncludePathInfo` | Whether to return the path from the HTTP request URL. | `true` |
| `IncludeQueryString` | Whether to return the query string parameters from the request URL. | `true` |
| `IncludeUserPrincipal` | Whether to return the username from [`HttpContext.User`](https://learn.microsoft.com/dotnet/api/system.security.claims.claimsprincipal). | `false` |
| `IncludeRemoteAddress` | Whether to return the IP address from the sender. | `false` |
| `IncludeSessionId` | Whether to return the user's session ID. | `false` |
| `IncludeTimeTaken` | Whether to return the request duration. | `true` |
| `Reverse` | Whether to return exchanges in reverse order (newest first). | `true` |

All request and response header values are redacted by default, except for the whitelisted entries below.
To return additional headers unredacted, add them to the `RequestHeaders` or `ResponseHeaders` arrays.

Whitelist of HTTP request headers:
- Accept
- Accept-Charset
- Accept-Encoding
- Accept-Language
- Allow
- Cache-Control
- Connection
- Content-Encoding
- Content-Length
- Content-Type
- Date
- DNT
- Expect
- Host
- Max-Forwards
- Range
- Sec-WebSocket-Extensions
- Sec-WebSocket-Version
- TE
- Trailer
- Transfer-Encoding
- Upgrade
- User-Agent
- Warning
- X-Requested-With
- X-UA-Compatible

Whitelist of HTTP response headers:
- Accept-Ranges
- Age
- Allow
- Alt-Svc
- Connection
- Content-Disposition
- Content-Language
- Content-Length
- Content-Location
- Content-Range
- Content-Type
- Date
- Expires
- Last-Modified
- Location
- Server
- Transfer-Encoding
- Upgrade
- X-Powered-By

> [!NOTE]
> The built-in whitelists are the same as those used by [ASP.NET Core](https://github.com/dotnet/aspnetcore/blob/release/8.0/src/Middleware/HttpLogging/src/HttpLoggingOptions.cs).

## Enable HTTP Access

The URL path to the endpoint is computed by combining the global `Management:Endpoints:Path` setting together with the `Path` setting described in the preceding section.
The default path is `/actuator/httpexchanges`.

See the [Exposing Endpoints](./using-endpoints.md#exposing-endpoints) and [HTTP Access](./using-endpoints.md#http-access) sections for the overall steps required to enable HTTP access to endpoints in an ASP.NET Core application.

To add the actuator to the service container and map its route, use the `AddHttpExchangesActuator` extension method.

Add the following code to `Program.cs` to use the actuator endpoint:

```csharp
using Steeltoe.Management.Endpoint.Actuators.HttpExchanges;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpExchangesActuator();
```

> [!TIP]
> It's recommended to use `AddAllActuators()` instead of adding individual actuators,
> which enables individually turning them on/off at runtime via configuration.

## Sample Output

This endpoint returns an array of exchanges.

The response will always be returned as JSON, like this:

```json
{
  "exchanges": [
    {
      "timeTaken": "PT0.1224915S",
      "timestamp": "2024-10-31T16:51:10.9575406Z",
      "request": {
        "method": "GET",
        "uri": "https://localhost:7105/hello?q=a",
        "headers": {
          "Accept": [
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
          ],
          "Host": [
            "localhost:7105"
          ],
          "User-Agent": [
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36"
          ],
          "Accept-Encoding": [
            "gzip, deflate, br, zstd"
          ],
          "Accept-Language": [
            "en-US,en;q=0.9,nl;q=0.8"
          ],
          "Upgrade-Insecure-Requests": [
            "******"
          ],
          "sec-ch-ua": [
            "******"
          ],
          "sec-ch-ua-mobile": [
            "******"
          ],
          "sec-ch-ua-platform": [
            "******"
          ],
          "sec-fetch-site": [
            "******"
          ],
          "sec-fetch-mode": [
            "******"
          ],
          "sec-fetch-user": [
            "******"
          ],
          "sec-fetch-dest": [
            "******"
          ],
          "priority": [
            "******"
          ]
        }
      },
      "response": {
        "status": 200,
        "headers": {
          "Content-Type": [
            "text/plain; charset=utf-8"
          ],
          "Date": [
            "Thu, 31 Oct 2024 16:51:10 GMT"
          ],
          "Server": [
            "Kestrel"
          ]
        }
      }
    }
  ]
}
```
