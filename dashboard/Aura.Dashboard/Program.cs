using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;
using Aura.Dashboard.Services;
using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;

var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MongoContext>();builder.Services.AddSingleton<IMediaRepository,MongoMediaRepository>();builder.Services.AddSingleton<IUserRepository,MongoUserRepository>();builder.Services.AddSingleton<IPlatformUserRepository,MongoPlatformUserRepository>();builder.Services.AddSingleton<IAuditRepository,MongoAuditRepository>();builder.Services.AddSingleton<IConfigurationRepository,MongoConfigurationRepository>();
builder.Services.AddHostedService<MongoSchemaInitializer>();
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "us-east-1")));
builder.Services.AddSingleton<IAssetStorage>(services =>
{
    var provider = (builder.Configuration["MediaStorage:Provider"] ?? "S3").Trim();
    return provider.ToUpperInvariant() switch
    {
        "S3" => ActivatorUtilities.CreateInstance<S3AssetStorage>(services),
        "AZURE" => ActivatorUtilities.CreateInstance<AzureBlobAssetStorage>(services),
        _ => throw new InvalidOperationException($"Unsupported media storage provider '{provider}'. Use 'S3' or 'Azure'.")
    };
});
builder.Services.Configure<YahooMailOptions>(builder.Configuration.GetSection(YahooMailOptions.SectionName));
builder.Services.Configure<DashboardLinkOptions>(builder.Configuration.GetSection(DashboardLinkOptions.SectionName));
builder.Services.AddSingleton<IEmailSender, YahooSmtpEmailSender>();
builder.Services.AddSingleton<DashboardLinkBuilder>();
builder.Services.AddSingleton<IResetDelivery, YahooResetDelivery>();
builder.Services.AddSingleton<IStaffInvitationDelivery, YahooStaffInvitationDelivery>();
builder.Services.AddScoped<IModerationService,ModerationService>();builder.Services.AddScoped<IUserManagementService,UserManagementService>();builder.Services.AddScoped<IMediaManagementService,MediaManagementService>();builder.Services.AddScoped<IReportingService,ReportingService>();builder.Services.AddScoped<IConfigurationManagementService,ConfigurationManagementService>();
builder.Services.AddScoped<IPlatformUserManagementService, PlatformUserManagementService>();
builder.Services.AddSingleton<ISecureTokenService, SecureTokenService>();
builder.Services.AddScoped<IStaffCredentialService, StaffCredentialService>();
builder.Services.AddScoped<IStaffBootstrapService, StaffBootstrapService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o=>{o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/AccessDenied";o.Cookie.Name="__Host-Aura.Management";o.Cookie.HttpOnly=true;o.Cookie.SecurePolicy=CookieSecurePolicy.Always;o.Cookie.SameSite=SameSiteMode.Strict;o.SlidingExpiration=true;o.ExpireTimeSpan=TimeSpan.FromMinutes(builder.Configuration.GetValue("Security:IdleMinutes",30));o.Events.OnValidatePrincipal=async c=>{var id=c.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;var version=c.Principal?.FindFirst("session_version")?.Value;var user=id is null?null:await c.HttpContext.RequestServices.GetRequiredService<IUserRepository>().FindAsync(id);if(user is null||user.IsBlocked||!user.IsReviewer||version!=user.SessionVersion.ToString())c.RejectPrincipal();};});
builder.Services.AddAuthorization(o=>{o.AddPolicy("Management",p=>p.RequireRole(Roles.Reviewer,Roles.Administrator));o.AddPolicy("Administrator",p=>p.RequireRole(Roles.Administrator));});
builder.Services.AddAntiforgery(o=>o.Cookie.SecurePolicy=CookieSecurePolicy.Always);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("staff-login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
    options.AddPolicy("first-admin-setup", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0
        }));
});
var app=builder.Build();if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseRateLimiter();app.UseAuthentication();app.UseAuthorization();app.MapControllerRoute("default","{controller=Dashboard}/{action=Index}/{id?}");app.Run();
public partial class Program;
