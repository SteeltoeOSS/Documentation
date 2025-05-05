using System.Text;

namespace Steeltoe.io;

public class DocsRedirectMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    ILogger<DocsRedirectMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalHost = context.Request.Host.Host.ToLower();

        var redirectUri = new StringBuilder();
        redirectUri.Append(Uri.UriSchemeHttps);
        redirectUri.Append(Uri.SchemeDelimiter);
        if (string.Equals(originalHost, "docs.steeltoe.io", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(originalHost, "docs-staging.steeltoe.io", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogTrace("Received request for {originalHost}", originalHost);
            redirectUri.Append(configuration["Docs:RedirectToHost"]);
#if DEBUG
            if (!string.IsNullOrEmpty(configuration["Docs:RedirectToPort"]))
            {
                redirectUri.Append(':');
                redirectUri.Append(configuration["Docs:RedirectToPort"]);
            }
#endif
            var path = context.Request.Path;
            if (path.StartsWithSegments("/api"))
            {
                if (path.StartsWithSegments("/api/browser"))
                {
                    path = path.ToString().Replace("/api/browser", "/api")
                        .Replace("/all", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/bootstrap", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/circuitbreaker", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/common", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/configuration", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/connectors", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/discovery", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/integration", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/logging", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/management", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/messaging", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/security", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("/stream", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace(".md", ".html");
                }
                else
                {
                    path = path.ToString().Replace("/api/", "/docs/", StringComparison.OrdinalIgnoreCase)
                        .Replace(".md", ".html");
                }
            }

            redirectUri.Append(path);
            var newLocation = redirectUri.ToString();
            logger.LogTrace("Redirecting to {newLocation}", newLocation);
            context.Response.Redirect(newLocation, permanent: true);
            return;
        }

        await next(context); // Call the next middleware
    }
}
