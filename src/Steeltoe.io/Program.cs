using Microsoft.AspNetCore.ResponseCompression;
using Steeltoe.io;
using Steeltoe.io.Components;
using Steeltoe.io.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents();

builder.Services.Configure<CalendarEventOptions>(builder.Configuration.GetSection("CalendarEvents"));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["image/svg+xml", "font/woff2"]);
});

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

app.UseMiddleware<DocsRedirectMiddleware>(builder.Configuration);

app.UseHttpsRedirection();

app.UseResponseCompression();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// UseStatusCodePagesWithReExecute must happen before anti-forgery to prevent errors
app.UseStatusCodePagesWithReExecute("/404");
app.UseAntiforgery();
app.MapRazorComponents<App>();


app.Run();
