# Aura Platform

Aura is a responsive media-hosting platform for videos, images, and audio. It includes creator uploads, administrative review, engagement, comments, search, and curated discovery experiences.

## Projects

- `backend/Aura.Api` — .NET 10 ASP.NET Core minimal API backed by MongoDB and Amazon S3.
- `frontend` — TypeScript, React, and Vite single-page frontend connected to the backend API.
- `doc/project-overview.md` — product and architecture requirements.

## Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend runs at `http://localhost:5173`. Set `VITE_API_URL` when the API is not available at the default `http://localhost:5080/api` address. The frontend reads media, authentication, configuration, upload, reaction, and comment data from the API; it does not ship demo media.

## Backend

Configure MongoDB, JWT, and the selected media-storage provider, then run:

```bash
cd backend/Aura.Api
dotnet restore
dotnet run
```

The API runs at `http://localhost:5080` under the included development launch profile. Replace the development JWT key before deployment and provide AWS credentials through the standard AWS credential chain.

### Media storage provider

Set `MediaStorage:Provider` to `S3` (the default) or `Azure` to select the media-storage implementation at startup. Environment variables use .NET's double-underscore notation:

```bash
# Amazon S3
MediaStorage__Provider=S3
AWS__Region=us-east-1
AWS__BucketName=aura-media

# Azure Blob Storage
MediaStorage__Provider=Azure
AzureStorage__ConnectionString='DefaultEndpointsProtocol=...'
AzureStorage__ContainerName=aura-media
```

Azure uploads use blocks with short-lived SAS URLs, allowing large media to continue using the frontend's chunked upload flow. The Azure Storage account must allow browser CORS requests from the frontend origin and expose the `PUT` method and `ETag` response header. The configured connection string must include credentials that can create the container, create SAS URLs, stage and commit blocks, read blobs, and delete blobs.

When the API runs in the Development environment, interactive Swagger documentation is available at `http://localhost:5080/swagger`. The underlying OpenAPI document is available at `http://localhost:5080/swagger/v1/swagger.json`.

The `GET /health` endpoint checks MongoDB and the active media-storage provider. It returns an overall status together with the status, description, and response duration of each dependency, and responds with an unhealthy HTTP status when either dependency cannot be reached.

### Backend architecture

The API follows a controller → service → repository structure:

- Controllers own HTTP routing, authorization declarations, and response codes.
- Services contain application rules and depend on interfaces for persistence and infrastructure.
- Repositories isolate MongoDB queries and updates from application logic.
- Infrastructure adapters isolate JWT creation, password hashing, email delivery, time, Amazon S3 storage, and Azure Blob Storage.

Dependencies are registered in `Program.cs`, which serves only as the application composition root and middleware pipeline.

## Verification

```bash
cd frontend
npm run lint
npm run typecheck
npm run build
npm run test:e2e
```

Backend unit tests enforce a minimum of 90% line coverage for the application service layer:

```bash
dotnet test AuraPlatform.slnx
```
