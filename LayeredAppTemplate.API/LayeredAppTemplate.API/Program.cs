using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using LayeredAppTemplate.API.Configuration;
using LayeredAppTemplate.API.Middlewares;
using LayeredAppTemplate.Application.Common.Interfaces;
using LayeredAppTemplate.Application.DTOs;
using LayeredAppTemplate.Application.Validators.UserValidators;
using LayeredAppTemplate.Infrastructure;
using LayeredAppTemplate.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Text;

// Kültürü invariant olarak ayarla
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------
// API Versioning
// ------------------------------------------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ------------------------------------------
// Swagger Configuration with JWT and Versioning
// ------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    // JWT Bearer Security Tanýmlamasý
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\nÖrnek: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
// Swagger versiyonlama için özel yapýlandýrma sýnýfýný ekle
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

// ------------------------------------------
// Serilog Configuration
// ------------------------------------------
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Error()
    .WriteTo.Console()
    .WriteTo.File(
         "logs/log.txt",
         rollingInterval: RollingInterval.Day,
         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
builder.Host.UseSerilog();

// ------------------------------------------
// JWT Authentication Configuration
// ------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ------------------------------------------
// Dependency Injection and Application Services
// ------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddApplicationDependencies(connectionString);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ------------------------------------------
// Controllers and FluentValidation
// ------------------------------------------
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateUserDtoValidator>());
builder.Services.AddEndpointsApiExplorer();

// ------------------------------------------
// Rate Limiting Configuration
// ------------------------------------------
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// ------------------------------------------
// Health Checks Configuration
// ------------------------------------------
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

// Health Checks UI Configuration
builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.SetEvaluationTimeInSeconds(60); // Her 60 saniyede bir kontrol
}).AddInMemoryStorage();

var app = builder.Build();

// Global Exception Handling middleware
app.UseGlobalExceptionHandler();

// Veritabaný baðlantýsýný kontrol et
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!dbContext.Database.CanConnect())
    {
        throw new Exception("Veritabanýna baðlanýlamadý! Lütfen baðlantý bilgisini kontrol edin.");
    }
}

// ------------------------------------------
// Middleware Pipeline Configuration
// ------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Rate Limiting middleware ekle
app.UseIpRateLimiting();

// Health Checks endpoint'ini ekle (örneðin /health)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

// Health Checks UI endpoint (örneðin /health-ui)
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
