using System.Text;
using Amazon;
using Amazon.S3;
using Aura.Api.Common;
using Aura.Api.Repositories;
using Aura.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IMongoHealthProbe>(services => services.GetRequiredService<MongoContext>());
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "us-east-1")));
builder.Services.AddSingleton<IMediaStorage>(services =>
{
    var provider = (builder.Configuration["MediaStorage:Provider"] ?? "S3").Trim();
    return provider.ToUpperInvariant() switch
    {
        "S3" => ActivatorUtilities.CreateInstance<S3MediaStorage>(services),
        "AZURE" => ActivatorUtilities.CreateInstance<AzureBlobMediaStorage>(services),
        _ => throw new InvalidOperationException($"Unsupported media storage provider '{provider}'. Use 'S3' or 'Azure'.")
    };
});
builder.Services.Configure<YahooMailOptions>(builder.Configuration.GetSection(YahooMailOptions.SectionName));
builder.Services.Configure<AuthLinkOptions>(builder.Configuration.GetSection(AuthLinkOptions.SectionName));
builder.Services.AddSingleton<AuthLinkBuilder>();
builder.Services.AddSingleton<IEmailSender, YahooSmtpEmailSender>();
builder.Services.AddSingleton<IEmailService, YahooEmailService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IMediaRepository, MediaRepository>();
builder.Services.AddSingleton<ICommentRepository, CommentRepository>();
builder.Services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IMediaService, MediaService>();
builder.Services.AddSingleton<IUploadService, UploadService>();
builder.Services.AddSingleton<ICommentService, CommentService>();
builder.Services.AddControllers(options => options.Filters.Add<ServiceExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb", tags: ["database"])
    .AddCheck<MediaStorageHealthCheck>("media-storage", tags: ["storage"]);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:5173")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            // Tokens issued before session versioning was introduced represent version zero.
            // This preserves existing sessions until that user's password is actually reset.
            var versionValue = context.Principal?.FindFirst("session_version")?.Value ?? "0";
            if (userId is null || !int.TryParse(versionValue, out var version) ||
                !await context.HttpContext.RequestServices.GetRequiredService<IUserRepository>().IsSessionValidAsync(userId, version))
                context.Fail("This session is no longer valid.");
        }
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            services = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration
                })
        }, context.RequestAborted);
    }
});
app.Run();

public partial class Program;
