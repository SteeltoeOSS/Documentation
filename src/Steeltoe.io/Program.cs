using Microsoft.AspNetCore.Rewrite;
using Steeltoe.io;
using Steeltoe.io.Components;
using Steeltoe.io.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents();

builder.Services.Configure<CalendarEventOptions>(builder.Configuration.GetSection("CalendarEvents"));

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

var rewriteOptions = new RewriteOptions()
    .AddRedirect("^docs/v3/obsolete", "docs/v4/welcome/migrate-quick-steps.html", 301)
    .AddRedirect("^circuit-breakers.*", "attic", 301)
    .AddRedirect("^steeltoe-circuitbreaker", "attic", 301)
    .AddRedirect("^event-driven", "attic", 301)
    .AddRedirect("^messaging.*", "attic", 301)
    .AddRedirect("^guides/messaging/rabbitmq.html", "attic", 301);

app.UseRewriter(rewriteOptions);

app.UseMiddleware<DocsRedirectMiddleware>(builder.Configuration);

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// UseStatusCodePagesWithReExecute must happen before anti-forgery to prevent errors
app.UseStatusCodePagesWithReExecute("/404");
app.UseAntiforgery();
app.MapRazorComponents<App>();


app.Run();
