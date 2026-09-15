# Aura Frontend

The Aura frontend is a responsive TypeScript and React single-page application built with Vite. It consumes the Aura backend API and contains no bundled demo media.

## Prerequisites

- A current Node.js LTS release
- npm
- The Aura backend API for live development

## Install and run

```bash
cd frontend
npm install
npm run dev
```

Vite serves the application at `http://localhost:5173` by default.

## Environment configuration

Create `frontend/.env.local` when the API is not hosted at its default address:

```dotenv
VITE_API_URL=http://localhost:5080/api
```

`VITE_API_URL` must include the API base path; the client normalizes an optional trailing slash. Vite embeds this value into the browser build, so use environment-specific values during deployment rather than storing secrets here. The frontend must never receive database, AWS, Azure account-key, or JWT-signing secrets.

The backend's `FrontendUrl` CORS setting must match the frontend origin. Direct browser uploads also require the selected S3 bucket or Azure Storage account to permit CORS requests from that origin.

## Available scripts

| Command | Purpose |
| --- | --- |
| `npm run dev` | Start the Vite development server. |
| `npm run typecheck` | Validate TypeScript without emitting files. |
| `npm run lint` | Run ESLint across source, configuration, and tests. |
| `npm run build` | Create an optimized production build in `dist/`. |
| `npm run test:e2e` | Run the Playwright browser suite. |

## Application structure

- `src/api.ts` is the typed backend client and coordinates direct multipart uploads.
- `src/context/AuthContext.tsx` manages the signed-in user and JWT-backed session.
- `src/components/` contains shared layout, media-card, and UI-state components.
- `src/pages/` contains route-level experiences for discovery, media, authentication, uploads, search, administration, legal pages, and My Uploads.
- `src/types.ts` defines the backend response shapes used by the UI.
- `src/styles.css` contains global and responsive styling.
- `tests/` contains Playwright end-to-end tests with mocked API responses.

## Authentication

The sign-in page sends the user's email and password to the backend. On success, the app stores the JWT and user summary in browser local storage and sends the token as a Bearer token on protected API calls. Signing out removes both values.

Do not put privileged information in the client-side user summary. Browser storage is accessible to frontend JavaScript, and authorization must always be enforced by the backend.

## Media uploads

Uploads are available to signed-in users when the backend configuration enables them:

1. The page submits the title, description, tags, file name, MIME type, and size to the API.
2. The API returns an upload ID and authorized part URLs for the configured S3 or Azure provider.
3. The browser uploads the file directly in 10 MB chunks and updates the visible progress indicator.
4. The app asks the API to complete the upload and displays the under-review confirmation.

Keep the page open until finalization completes. For cross-origin direct uploads, configure storage CORS to allow the frontend origin, `PUT`, required headers, and the `ETag` response header.

## Media playback and sharing

Video and audio use native browser media controls; images use responsive image rendering. Missing or inaccessible media displays an unavailable state. The Share action uses the Web Share API when supported and falls back to copying the direct content URL.

## Error handling

Backend and network failures are shown in a global dismissible alert. Pages also provide contextual empty, loading, unavailable, and inline error states. API errors should be allowed to pass through `src/api.ts` so users receive consistent feedback.

## End-to-end tests

Install the Playwright browser once per environment, then run the suite:

```bash
cd frontend
npx playwright install chromium
npm run test:e2e
```

The Playwright configuration starts a preview development server at `http://127.0.0.1:4173`. Tests mock backend requests, so MongoDB and cloud-storage credentials are not required.

Run all frontend checks before submitting changes:

```bash
npm run lint
npm run typecheck
npm run build
npm run test:e2e
```

## Production build

```bash
cd frontend
VITE_API_URL=https://api.example.com/api npm run build
```

Deploy the generated `dist/` directory to a static host. Configure the host to route unknown application paths, such as `/media/{id}`, back to `index.html` so pasted direct-content URLs load the SPA correctly.
