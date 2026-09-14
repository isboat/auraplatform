using System.Security.Claims;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Aura.Api.Models;
using Aura.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<IEmailService, LoggingEmailService>();
builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "us-east-1")));
builder.Services.AddSingleton<MediaStorage>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(builder.Configuration["FrontendUrl"] ?? "http://localhost:5173")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

static string UserId(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? throw new UnauthorizedAccessException();
static async Task<PlatformConfiguration> Settings(MongoContext db) => await db.Configuration.Find(x => x.Id == "platform").FirstOrDefaultAsync() ?? new();
static object MediaView(MediaDocument media, MediaStorage storage, long comments) => new { media.Id, media.Title, media.Description, media.Tags, media.MediaType, media.ReviewStatus, media.CreatedAt, media.OwnerName, media.Views, Likes = media.LikeCount, Dislikes = media.DislikeCount, Comments = comments, Url = media.ReviewStatus == "Approved" ? storage.ReadUrl(media.ObjectKey) : null };

app.MapGet("/api/configuration", async (MongoContext db) => Results.Ok(await Settings(db)));
app.MapPut("/api/configuration", async (ConfigurationRequest request, MongoContext db) =>
{
    var value = new PlatformConfiguration { RegistrationEnabled = request.RegistrationEnabled, UploadsEnabled = request.UploadsEnabled };
    await db.Configuration.ReplaceOneAsync(x => x.Id == value.Id, value, new ReplaceOptions { IsUpsert = true });
    return Results.Ok(value);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/api/auth/register", async (RegisterRequest request, HttpContext http, MongoContext db, IEmailService email) =>
{
    if (!(await Settings(db)).RegistrationEnabled) return Results.Problem("Registration is currently unavailable.", statusCode: 403);
    var normalized = request.Email.Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Password) || !normalized.Contains('@')) return Results.ValidationProblem(new Dictionary<string, string[]> { ["registration"] = ["Name, a valid email, and password are required."] });
    if (await db.Users.Find(x => x.Email == normalized).AnyAsync()) return Results.Conflict(new { message = "An account with this email already exists." });
    var user = new UserDocument { Name = request.Name.Trim(), Email = normalized, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password) };
    await db.Users.InsertOneAsync(user);
    var url = $"{http.Request.Scheme}://{http.Request.Host}/api/auth/verify?token={user.VerificationToken}";
    await email.SendVerificationAsync(user.Email, url);
    return Results.Accepted(value: new { message = "Check your email to complete account setup." });
});

app.MapGet("/api/auth/verify", async (string token, MongoContext db) =>
{
    var result = await db.Users.UpdateOneAsync(x => x.VerificationToken == token, Builders<UserDocument>.Update.Set(x => x.EmailVerified, true).Set(x => x.VerificationToken, ""));
    return result.ModifiedCount == 1 ? Results.Ok(new { message = "Your account is verified." }) : Results.NotFound(new { message = "Verification link is invalid or expired." });
});

app.MapPost("/api/auth/login", async (LoginRequest request, MongoContext db, TokenService tokens) =>
{
    var user = await db.Users.Find(x => x.Email == request.Email.Trim().ToLowerInvariant()).FirstOrDefaultAsync();
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return Results.Unauthorized();
    if (!user.EmailVerified) return Results.Problem("Verify your email before signing in.", statusCode: 403);
    return Results.Ok(new { token = tokens.Create(user), user = new { user.Id, user.Name, user.Email, user.IsAdministrator } });
});

app.MapGet("/api/media/home", async (MongoContext db, MediaStorage storage) =>
{
    var approved = db.Media.Find(x => x.ReviewStatus == "Approved");
    async Task<List<object>> Load(SortDefinition<MediaDocument> sort) { var rows = await approved.Sort(sort).Limit(10).ToListAsync(); return rows.Select(x => MediaView(x, storage, 0)).ToList(); }
    return Results.Ok(new { latest = await Load(Builders<MediaDocument>.Sort.Descending(x => x.CreatedAt)), mostViewed = await Load(Builders<MediaDocument>.Sort.Descending(x => x.Views)), mostLiked = await Load(Builders<MediaDocument>.Sort.Descending(x => x.LikeCount)) });
});

app.MapGet("/api/media/search", async (string? tag, MongoContext db, MediaStorage storage) =>
{
    var filter = string.IsNullOrWhiteSpace(tag) ? Builders<MediaDocument>.Filter.Empty : Builders<MediaDocument>.Filter.AnyEq(x => x.Tags, tag.Trim().ToLowerInvariant());
    var rows = await db.Media.Find(filter).SortByDescending(x => x.CreatedAt).Limit(50).ToListAsync();
    return Results.Ok(rows.Select(x => MediaView(x, storage, 0)));
});

app.MapGet("/api/media/{id}", async (string id, MongoContext db, MediaStorage storage) =>
{
    var media = await db.Media.Find(x => x.Id == id).FirstOrDefaultAsync();
    if (media is null) return Results.NotFound();
    await db.Media.UpdateOneAsync(x => x.Id == id, Builders<MediaDocument>.Update.Inc(x => x.Views, 1));
    var comments = await db.Comments.CountDocumentsAsync(x => x.MediaId == id);
    media.Views++;
    return Results.Ok(MediaView(media, storage, comments));
});

app.MapPost("/api/media/uploads", async (UploadRequest request, ClaimsPrincipal principal, MongoContext db, MediaStorage storage) =>
{
    if (!(await Settings(db)).UploadsEnabled) return Results.Problem("Uploads are currently unavailable.", statusCode: 403);
    if (request.Description.Length > 255) return Results.ValidationProblem(new Dictionary<string, string[]> { ["description"] = ["Description cannot exceed 255 characters."] });
    var currentUserId = UserId(principal);
    var user = await db.Users.Find(x => x.Id == currentUserId).FirstAsync();
    var key = $"{user.Id}/{Guid.NewGuid():N}/{Path.GetFileName(request.FileName)}";
    var transfer = await storage.BeginAsync(key, request.ContentType, request.FileSize);
    var media = new MediaDocument { OwnerId = user.Id!, OwnerName = user.Name, Title = string.IsNullOrWhiteSpace(request.Title) ? DateTime.UtcNow.ToString("yyyy-MM-dd hh mm ss") : request.Title.Trim(), Description = request.Description, Tags = request.Tags?.Select(x => x.Trim().ToLowerInvariant()).Where(x => x.Length > 0).Distinct().ToList() ?? [], MediaType = request.ContentType.Split('/')[0], ObjectKey = key };
    await db.Media.InsertOneAsync(media);
    return Results.Ok(new { media.Id, transfer.uploadId, transfer.urls, message = "Your media is under review." });
}).RequireAuthorization();

app.MapPost("/api/media/{id}/complete", async (string id, CompleteUploadRequest request, ClaimsPrincipal principal, MongoContext db, MediaStorage storage) =>
{
    var currentUserId = UserId(principal);
    var media = await db.Media.Find(x => x.Id == id && x.OwnerId == currentUserId).FirstOrDefaultAsync();
    if (media is null) return Results.NotFound();
    await storage.CompleteAsync(media.ObjectKey, request.UploadId, request.Parts.Select(x => new PartETag(x.PartNumber, x.ETag)));
    return Results.Ok(new { message = "Your media is under review." });
}).RequireAuthorization();

app.MapGet("/api/media/mine", async (ClaimsPrincipal principal, MongoContext db, MediaStorage storage) =>
{
    var userId = UserId(principal);
    var rows = await db.Media.Find(x => x.OwnerId == userId).SortByDescending(x => x.CreatedAt).ToListAsync();
    var result = new List<object>();
    foreach (var row in rows) result.Add(MediaView(row, storage, await db.Comments.CountDocumentsAsync(x => x.MediaId == row.Id)));
    return Results.Ok(result);
}).RequireAuthorization();

app.MapDelete("/api/media/{id}", async (string id, ClaimsPrincipal principal, MongoContext db, MediaStorage storage) =>
{
    var currentUserId = UserId(principal);
    var media = await db.Media.FindOneAndDeleteAsync(x => x.Id == id && x.OwnerId == currentUserId);
    if (media is null) return Results.NotFound();
    await storage.DeleteAsync(media.ObjectKey);
    await db.Comments.DeleteManyAsync(x => x.MediaId == id);
    return Results.NoContent();
}).RequireAuthorization();

app.MapPost("/api/media/{id}/reaction", async (string id, ReactionRequest request, ClaimsPrincipal principal, MongoContext db) =>
{
    var userId = UserId(principal);
    var media = await db.Media.Find(x => x.Id == id && x.ReviewStatus == "Approved").FirstOrDefaultAsync();
    if (media is null) return Results.NotFound();
    var hadLike = media.Likes.Contains(userId);
    var hadDislike = media.Dislikes.Contains(userId);
    var update = request.Like
        ? Builders<MediaDocument>.Update.AddToSet(x => x.Likes, userId).Pull(x => x.Dislikes, userId).Inc(x => x.LikeCount, hadLike ? 0 : 1).Inc(x => x.DislikeCount, hadDislike ? -1 : 0)
        : Builders<MediaDocument>.Update.AddToSet(x => x.Dislikes, userId).Pull(x => x.Likes, userId).Inc(x => x.DislikeCount, hadDislike ? 0 : 1).Inc(x => x.LikeCount, hadLike ? -1 : 0);
    var result = await db.Media.UpdateOneAsync(x => x.Id == id && x.ReviewStatus == "Approved", update);
    return result.MatchedCount == 1 ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();

app.MapGet("/api/media/{id}/comments", async (string id, int? limit, DateTime? before, MongoContext db) =>
{
    var take = Math.Clamp(limit ?? 10, 1, 50);
    var filter = Builders<CommentDocument>.Filter.Eq(x => x.MediaId, id);
    if (before.HasValue) filter &= Builders<CommentDocument>.Filter.Lt(x => x.CreatedAt, before.Value);
    return Results.Ok(await db.Comments.Find(filter).SortByDescending(x => x.CreatedAt).Limit(take).ToListAsync());
});

app.MapPost("/api/media/{id}/comments", async (string id, CommentRequest request, ClaimsPrincipal principal, MongoContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Body)) return Results.BadRequest(new { message = "Comment is required." });
    var currentUserId = UserId(principal);
    var user = await db.Users.Find(x => x.Id == currentUserId).FirstAsync();
    var comment = new CommentDocument { MediaId = id, UserId = user.Id!, UserName = user.Name, Body = request.Body.Trim() };
    await db.Comments.InsertOneAsync(comment);
    return Results.Created($"/api/media/{id}/comments", comment);
}).RequireAuthorization();

app.MapPut("/api/admin/media/{id}/review", async (string id, string status, MongoContext db) =>
{
    if (status is not ("Approved" or "Rejected")) return Results.BadRequest();
    var result = await db.Media.UpdateOneAsync(x => x.Id == id, Builders<MediaDocument>.Update.Set(x => x.ReviewStatus, status));
    return result.MatchedCount == 1 ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();
