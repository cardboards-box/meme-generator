using CardboardBox;
using MemeGenerator.Services;
using MemeGenerator.Web;
using MemeGenerator.Web.Background;
using MemeGenerator.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddSerilog()
    .AddMemeServices(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddCustomSwaggerGen()
    .AddServerServices()
    .AddTelemetry()
    .AddResponseCaching();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services
    .AddHostedService<TemplatesBackgroundService>()
    .AddHostedService<GenerationBackgroundService>()
    .AddHostedService<CleanupBackgroundService>();

var app = builder.Build();

app.RegisterBoxing();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseSwagger()
    .UseSwaggerUI()
    .UseCors(c => c
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin()
        .WithExposedHeaders("Content-Disposition"))
    .UseStaticFiles()
    .UseAntiforgery()
    .UseResponseCaching();

app.MapPrometheusScrapingEndpoint();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MemeGenerator.Web.Client._Imports).Assembly);

app.Run();
