using Steeltoe.Client.Components;
using Steeltoe.Client.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "DocumentationPolicy",
           policy =>
           {
               policy.AllowAnyOrigin().AllowAnyHeader().WithMethods("GET");
           });
});

builder.Services.Configure<CalendarEventOptions>(builder.Configuration.GetSection("CalendarEvents"));
builder.Services.Configure<DocsSiteOptions>(builder.Configuration.GetSection("DocsSite"));

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

app.UseStaticFiles();

app.UseRouting();
app.UseAntiforgery();
app.UseCors("DocumentationPolicy");
app.MapRazorComponents<App>();

app.Run();
