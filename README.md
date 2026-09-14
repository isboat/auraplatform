# Aura Platform

Aura is a responsive media-hosting platform for videos, images, and audio. It includes creator uploads, administrative review, engagement, comments, search, and curated discovery experiences.

## Projects

- `backend/Aura.Api` — .NET 10 ASP.NET Core minimal API backed by MongoDB and Amazon S3.
- `frontend` — React and Vite single-page frontend.
- `doc/project-overview.md` — product and architecture requirements.

## Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend runs at `http://localhost:5173`. Representative local content allows the interface to be explored without backend infrastructure.

## Backend

Configure MongoDB, JWT, AWS region, and S3 settings, then run:

```bash
cd backend/Aura.Api
dotnet restore
dotnet run
```

The API runs at `http://localhost:5080` under the included development launch profile. Replace the development JWT key before deployment and provide AWS credentials through the standard AWS credential chain.

## Verification

```bash
cd frontend
npm run lint
npm run build
npm run test:e2e
```
