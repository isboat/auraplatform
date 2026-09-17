# Aura Backend API

The Aura backend is an ASP.NET Core controller API targeting .NET 10. It owns authentication, media metadata, upload coordination, content discovery and review, reactions, comments, platform configuration, and dependency health reporting.

## Prerequisites

- .NET 10 SDK
- MongoDB
- One media-storage provider:
  - Amazon S3 and AWS credentials, or
  - Azure Blob Storage and an account-key or service-SAS connection string

## Run locally

From the repository root:

```bash
dotnet restore AuraPlatform.slnx
dotnet run --project backend/Aura.Api
```

The Development launch profile listens on `http://localhost:5080`. Swagger UI is available at `http://localhost:5080/swagger`, and its OpenAPI document is available at `http://localhost:5080/swagger/v1/swagger.json`.

## Configuration

Settings can come from `backend/Aura.Api/appsettings.json`, environment-specific settings, user secrets, or environment variables. Use double underscores for nested environment-variable keys.

| Setting | Required | Purpose |
| --- | --- | --- |
| `MongoDb:ConnectionString` | Yes | MongoDB server connection string. |
| `MongoDb:DatabaseName` | Yes | Database containing users, media, comments, and platform configuration. |
| `Jwt:Issuer` | Yes | Expected JWT issuer. |
| `Jwt:Audience` | Yes | Expected JWT audience. |
| `Jwt:Key` | Yes | Signing key; replace the development value with a secret of at least 32 characters. |
| `FrontendUrl` | No | Allowed CORS origin; defaults to `http://localhost:5173`. |
| `MediaStorage:Provider` | Yes | `S3` or `Azure`; defaults to `S3`. |
| `AWS:Region` | For S3 | AWS region containing the bucket. |
| `AWS:BucketName` | For S3 | Bucket used for media objects. |
| `AzureStorage:ConnectionString` | For Azure | Connection string containing `AccountKey` or `SharedAccessSignature`. |
| `AzureStorage:ContainerName` | For Azure | Blob container; defaults to `aura-media`. |
| `YahooMail:EmailAddress` | Yes | Yahoo address used as the SMTP sender and username. |
| `YahooMail:Passkey` | Yes | Yahoo app password used to authenticate to SMTP. |

Do not commit production connection strings, JWT keys, account keys, or SAS tokens. Prefer environment variables or a managed secret store.

### Amazon S3

```bash
export MediaStorage__Provider=S3
export AWS__Region=us-east-1
export AWS__BucketName=aura-media
dotnet run --project backend/Aura.Api
```

AWS credentials are resolved through the standard AWS credential chain. The application needs permission to initiate and complete multipart uploads, generate access URLs, delete objects, and read bucket location for health checks.

### Azure Blob Storage

```bash
export MediaStorage__Provider=Azure
export AzureStorage__ConnectionString='DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net'
export AzureStorage__ContainerName=aura-media
dotnet run --project backend/Aura.Api
```

Azure uploads use staged blocks. With an account-key connection string, the backend generates short-lived SAS URLs. With a service-SAS connection string, the existing `SharedAccessSignature` is reused and must grant the required create, write, read, and delete permissions. The storage account must allow CORS requests from the frontend origin, including `PUT`, required request headers, and the `ETag` response header.

## Authentication and authorization

Registration creates an unverified account and sends a verification link through `IEmailService`. The Yahoo adapter connects to `smtp.mail.yahoo.com` with TLS and authenticates with the configured email address and app password. Supply both settings through a secret store or environment variables (for example, `YahooMail__EmailAddress` and `YahooMail__Passkey`); never commit the passkey. Login returns a JWT after verification. Send it to protected routes as:

```http
Authorization: Bearer <token>
```

Routes marked as administrative require the JWT `Admin` role.

## Main API routes

| Method and route | Access | Purpose |
| --- | --- | --- |
| `POST /api/auth/register` | Public | Register an account. |
| `GET /api/auth/verify?token=...` | Public | Verify an email address. |
| `POST /api/auth/login` | Public | Obtain a JWT. |
| `GET /api/media/home` | Public | Get latest approved, most-viewed, and most-liked media. |
| `GET /api/media/search?tag=...` | Public | Search approved media. |
| `GET /api/media/{id}` | Public | Get a media page and record its view. |
| `GET /api/media/mine` | Signed in | List the current user's uploads. |
| `POST /api/media/uploads` | Signed in | Start a multipart or block upload. |
| `POST /api/media/{id}/complete` | Signed in | Finalize an upload. |
| `DELETE /api/media/{id}` | Owner | Delete an upload and its stored asset. |
| `POST /api/media/{id}/reaction` | Signed in | Like or dislike approved media. |
| `GET /api/media/{id}/comments` | Public | Read a batch of comments. |
| `POST /api/media/{id}/comments` | Signed in | Add a comment. |
| `GET /api/configuration` | Public | Read public platform feature settings. |
| `PUT /api/configuration` | Admin | Enable or disable registration and uploads. |
| `PUT /api/admin/media/{id}/review?status=Approved` | Admin | Approve or reject an upload. |
| `GET /health` | Public | Check MongoDB and the active media-storage provider. |

Use Swagger for the authoritative request and response schemas.

## Upload flow

1. The authenticated frontend sends metadata to `POST /api/media/uploads`.
2. The backend creates an `InReview` media document and returns provider-authorized part URLs.
3. The browser uploads 10 MB chunks directly to S3 or Azure Blob Storage.
4. The frontend sends uploaded part information to `POST /api/media/{id}/complete`.
5. The backend completes the multipart upload or commits the Azure block list.

The client must keep the returned `uploadId`, part order, and ETags intact. New media remains under review until an administrator approves it.

## Architecture

The API follows a controller → service → repository design:

- `Controllers/` defines HTTP behavior and authorization.
- `Services/` contains application rules and infrastructure abstractions.
- `Repositories/` isolates MongoDB persistence.
- `Models/` contains persisted documents and API contracts.
- `Common/` contains claims and error-handling helpers.

`Program.cs` is the composition root and selects the configured `IMediaStorage` implementation. Services depend on interfaces rather than specific cloud SDKs.

## Health checks

`GET /health` returns an aggregate status plus individual MongoDB and active-storage results. It returns a non-success status when either dependency is unavailable. Storage health automatically follows `MediaStorage:Provider`.

## Tests

From the repository root:

```bash
dotnet test AuraPlatform.slnx
dotnet build AuraPlatform.slnx --configuration Release
```

The xUnit suite is in `backend/Aura.Api.Tests`. Coverlet enforces at least 90% line coverage for the configured application-service classes.

## GitHub Actions deployment

`.github/workflows/backend-api.yml` runs separate build and test jobs for pull
requests and pushes that change the backend. After both jobs succeed on `main`, its
deploy job publishes the release artifact to Azure Web App.

Configure these repository Actions values:

- Variable `AZURE_BACKEND_APP_NAME`: the Azure Web App resource name.
- Secret `AZURE_BACKEND_PUBLISH_PROFILE`: the complete publish-profile XML
  downloaded from that Web App.

Set all runtime values described in [Configuration](#configuration) as Azure Web App
application settings. At minimum, replace the development JWT key and provide
production MongoDB and media-storage credentials. Also set `FrontendUrl` to the
deployed frontend origin. The workflow intentionally does not handle runtime secrets.
Manual runs are available through **Actions → Backend API → Run workflow**; deployment
still occurs only when the run targets `main`.
