# Configuration-based discovery client

The simplest form of service discovery is by storing the list of app instances in .NET configuration.

## Configuration settings

An app instance in configuration contains the following keys:

| Key | Description |
| --- | --- | --- |
| `ServiceId` | The app friendly name. |
| `Host` | The hostname or IP address of the service instance. |
| `Port` | The port number the service instance is listening on. |
| `IsSecure` | Whether to use HTTP or HTTPS to access the service instance. |

For example, the `appsettings.json` file below adds one instance of "billingService" and two instances of "shippingService".

```json
{
  "Discovery": {
    "Services": [
      {
        "ServiceId": "billingService",
        "Host": "192.168.0.1",
        "Port": "5000",
        "IsSecure": false
      },
      {
        "ServiceId": "shippingService",
        "Host": "one.internal.shipping.company.com",
        "Port": "888",
        "IsSecure": true
      },
      {
        "ServiceId": "shippingService",
        "Host": "two.internal.shipping.company.com",
        "Port": "999",
        "IsSecure": true
      }
    ]
  }
}
```

Using the above configuration, sending an HTTP request to `http://shippingService/api?id=123` with Steeltoe service discovery
activated would send the request to either:
- `https://one.internal.shipping.company.com:888/api?id=123`
- `https://two.internal.shipping.company.com:999/api?id=123`

The chosen instance depends on the load balancer strategy, which defaults to random.
