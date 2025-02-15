using CardboardBox.Extensions.Requesting;
using CardboardBox.Extensions;
using ImageBox.Core.Config;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using static System.Net.Mime.MediaTypeNames;
using MemeGenerator.Web.Client.Services;
using MemeGenerator.Web.Controllers;

namespace MemeGenerator.Web;

public static class Extensions
{
    public const int CACHE_AGE_YEAR = 60 * 60 * 24 * 365;

    public static IServiceCollection AddCustomSwaggerGen(this IServiceCollection services)
    {
        return services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var commentFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var file in commentFiles)
                c.IncludeXmlComments(file, file.Contains(".Api") || file.Contains(".Web"));
        });
    }

    public static IApplicationBuilder RegisterBoxing(this WebApplication app)
    {
        app.UseExceptionHandler(err => err.Run(async ctx =>
        {
            if (ctx.Response.HasStarted || !ctx.Request.Path.ToString().ContainsIc("api")) return;

            Exception? resolveException(WebApplication app)
            {
                if (!app.Environment.IsDevelopment()) return null;

                var feature = ctx.Features.Get<IExceptionHandlerFeature>();
                if (feature != null && feature.Error != null)
                    return feature.Error;

                feature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                return feature?.Error;
            };

            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = Application.Json;
            var error = resolveException(app) ?? new Exception("An error has occurred, please contact an administrator for more information");

            await ctx.Response.WriteAsJsonAsync(Boxed.Exception(error));
        }));

        app.Use(async (ctx, next) =>
        {
            await next();

            if (ctx.Response.StatusCode != StatusCodes.Status401Unauthorized ||
                ctx.Response.HasStarted ||
                !ctx.Request.Path.ToString().ContainsIc("api")) return;

            ctx.Response.ContentType = Application.Json;
            await ctx.Response.WriteAsJsonAsync(Boxed.Unauthorized());
        });

        return app;
    }

    public static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddPrometheusExporter();

                builder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
                builder.AddView("http.server.request.duration",
                    new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries =
                        [
                            0, 0.005, 0.01, 0.025, 0.05,
                            0.075, 0.1, 0.25, 0.5, 0.75,
                            1, 2.5, 5, 7.5, 10
                        ]
                    });
            });

        return services;
    }

    public static IServiceCollection AddServerServices(this IServiceCollection services)
    {
        RenderConfig.DefaultAssemblyFactory = ImageBox.DiExtensions.ImageBoxAssemblies;
        return services
            .AddSharedServices()
            .AddTransient<IMemesApiService, MemesApiService>()
            .AddTransient<ITemplatesApiService, TemplatesApiService>();
    }
}
