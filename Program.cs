using WindowsFolderAgent.Models;
using WindowsFolderAgent.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:3030");

var allowedOrigins =
    builder.Configuration
        .GetSection("AgentSettings:AllowedOrigins")
        .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type", "X-Agent-Request");
    });
});

builder.Services.AddSingleton<FolderService>();

var app = builder.Build();

app.UseCors("ReactApp");

// Verificar el origen de las solicitudes a la API.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var origin = context.Request.Headers.Origin.ToString();

        if (string.IsNullOrWhiteSpace(origin) ||
            !allowedOrigins.Contains(
                origin,
                StringComparer.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Origen no autorizado."
            });

            return;
        }
    }

    await next();
});

app.MapGet("/api/agent/status", () =>
{
    return Results.Ok(new
    {
        success = true,
        name = "WindowsFolderAgent",
        version = "1.0.0",
        status = "Running"
    });
});

app.MapPost("/api/folders/open",
    (OpenFolderRequest request, FolderService folderService) =>
{
    try
    {
        folderService.OpenFolder(request.FolderPath);

        return Results.Ok(new
        {
            success = true,
            message = "Carpeta abierta correctamente."
        });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(
            new { success = false, message = ex.Message },
            statusCode: 403
        );
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = ex.Message
        });
    }
});

app.Run();