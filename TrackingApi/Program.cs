using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrackingApi.Features.Legacy;
using TrackingApi.Infrastructure.Auth;
using TrackingApi.Infrastructure.Configuration;
using TrackingApi.Infrastructure.Data;
using TrackingApi.Infrastructure.Health;
using TrackingApi.Infrastructure.Requests;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

var apiBinding = builder.Configuration.GetSection(ApiBindingOptions.SectionName).Get<ApiBindingOptions>();
if (!string.IsNullOrWhiteSpace(apiBinding?.Url))
{
    builder.WebHost.UseUrls(apiBinding.Url);
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestLineSize = 1024 * 1024;
    options.Limits.MaxRequestHeadersTotalSize = 1024 * 1024;
    options.Limits.MaxRequestBodySize = 256 * 1024 * 1024;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 256 * 1024 * 1024;
    options.ValueLengthLimit = int.MaxValue;
    options.KeyLengthLimit = 4096;
    options.ValueCountLimit = 1024;
    options.MultipartHeadersLengthLimit = 32 * 1024;
});

builder.Services.Configure<ApiBindingOptions>(builder.Configuration.GetSection(ApiBindingOptions.SectionName));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Gestion")
    ?? throw new InvalidOperationException("Missing connection string 'Gestion'.");

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Missing JWT configuration.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException("JWT key must be configured.");
}

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>("sql");

builder.Services.AddDbContext<TrackingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<LegacyTrackingService>();
builder.Services.AddScoped<LegacyRequestReader>();

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtOptions.Issuer),
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtOptions.Audience),
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tracking API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
