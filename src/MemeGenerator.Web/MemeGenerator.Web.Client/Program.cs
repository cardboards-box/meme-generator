using MemeGenerator.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddClientServices();

await builder.Build().RunAsync();
