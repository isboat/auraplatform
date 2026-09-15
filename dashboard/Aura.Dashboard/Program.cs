using Aura.Dashboard.Domain;
using Aura.Dashboard.Repositories;
using Aura.Dashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoContext>();builder.Services.AddSingleton<IMediaRepository,MongoMediaRepository>();builder.Services.AddSingleton<IUserRepository,MongoUserRepository>();builder.Services.AddSingleton<IAuditRepository,MongoAuditRepository>();
builder.Services.AddHostedService<MongoSchemaInitializer>();
builder.Services.AddSingleton<IAssetStorage,ConfiguredAssetStorage>();builder.Services.AddSingleton<IResetDelivery,LoggingResetDelivery>();builder.Services.AddScoped<IModerationService,ModerationService>();builder.Services.AddScoped<IUserManagementService,UserManagementService>();builder.Services.AddScoped<IMediaManagementService,MediaManagementService>();builder.Services.AddScoped<IReportingService,ReportingService>();
builder.Services.AddScoped<IStaffBootstrapService, StaffBootstrapService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o=>{o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/AccessDenied";o.Cookie.Name="__Host-Aura.Management";o.Cookie.HttpOnly=true;o.Cookie.SecurePolicy=CookieSecurePolicy.Always;o.Cookie.SameSite=SameSiteMode.Strict;o.SlidingExpiration=true;o.ExpireTimeSpan=TimeSpan.FromMinutes(builder.Configuration.GetValue("Security:IdleMinutes",30));o.Events.OnValidatePrincipal=async c=>{var id=c.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;var version=c.Principal?.FindFirst("session_version")?.Value;var user=id is null?null:await c.HttpContext.RequestServices.GetRequiredService<IUserRepository>().FindAsync(id);if(user is null||user.IsBlocked||!user.IsReviewer||version!=user.SessionVersion.ToString())c.RejectPrincipal();};});
builder.Services.AddAuthorization(o=>{o.AddPolicy("Management",p=>p.RequireRole(Roles.Reviewer,Roles.Administrator));o.AddPolicy("Administrator",p=>p.RequireRole(Roles.Administrator));});
builder.Services.AddAntiforgery(o=>o.Cookie.SecurePolicy=CookieSecurePolicy.Always);
var app=builder.Build();if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseAuthentication();app.UseAuthorization();app.MapControllerRoute("default","{controller=Dashboard}/{action=Index}/{id?}");app.Run();
public partial class Program;
