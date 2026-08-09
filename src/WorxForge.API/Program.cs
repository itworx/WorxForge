using Microsoft.OpenApi;
using Serilog;
using WorxForge.API.Configuration;
using WorxForge.API.Extensions;
using WorxForge.API.Middlewares;
using WorxForge.API.Services;
using WorxForge.Core.Configuration;
using WorxForge.Infrastructure.Configuration;
using WorxForge.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT token only (no Bearer prefix). Swagger adds it automatically."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.OperationFilter<BearerSecurityOperationFilter>();
});
builder.Services.AddOpenApi();
builder.Services.AddWorxForgeCors(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddJwtAccessTokenAuthentication();
builder.Services.AddWorxForgeAuthorization();
builder.Services.AddWorxForgeCore();
builder.Services.AddWorxForgeInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHostedService<NotificationDeliveryBackgroundService>();

var app = builder.Build();

await app.Services.SeedWorxForgeDataAsync();

app.UseWorxForgePipeline();
app.MapWorxForgeEndpoints();

app.Run();
