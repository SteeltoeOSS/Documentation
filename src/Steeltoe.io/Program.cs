using Docfx.Dotnet;
using Steeltoe.io.Components;
using Steeltoe.io.Models;

var builder = WebApplication.CreateBuilder(args);

// run "docfx build" for local development
#if DEBUG
await DotnetApiCatalog.GenerateManagedReferenceYamlFiles("docfx.json");
await Docfx.Docset.Build("docfx.json");
#endif

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

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
// UseStatusCodePagesWithReExecute must be added before Anti-forgery
app.UseStatusCodePagesWithReExecute("/404");
app.UseAntiforgery();
app.MapRazorComponents<App>();

app.Run();
