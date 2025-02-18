using HighlightBlazor;
using MemeGenerator.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddClientServices()
    .AddHighlight();

await builder.Build().RunAsync();
