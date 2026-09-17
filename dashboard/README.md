# Aura Management Dashboard

The dashboard is an isolated ASP.NET Core MVC staff application implementing the workflows in `doc/management-dashboard.md`.

Each C# type has its own source file. Keep new domain models, interfaces,
repositories, services, controllers, and view models separated in the same way so
individual responsibilities remain easy to navigate and review.

## Run

1. Install the .NET 10 SDK and start MongoDB.
2. Supply `ConnectionStrings__MongoDb`, `Mongo__Database`, and the same media-storage configuration used by the API through environment-specific configuration or a secret store. Set `MediaStorage__Provider` to `S3` with `AWS__Region` and `AWS__BucketName`, or to `Azure` with `AzureStorage__ConnectionString` and `AzureStorage__ContainerName`.
3. Run `dotnet run --project dashboard/Aura.Dashboard`.

Only verified, active users with `ContentReviewer` or `Administrator` access can authenticate. The dashboard generates the same one-hour signed media URLs as the API and deletes assets through the configured provider. Production deployments must replace the development reset adapter with the mail delivery used by the API.

Dashboard identities are stored in the dedicated MongoDB `staffuser` collection, not
the public API's `users` collection. At application startup the dashboard ensures the
collection exists by creating a unique email index and a management state/roles index.

## Create the first administrator

Set a long, random one-time token through environment configuration and start the
dashboard:

```sh
export Bootstrap__Token="$(openssl rand -base64 32)"
dotnet run --project dashboard/Aura.Dashboard
```

Open `/Setup`, enter the token and create the initial administrator. The endpoint
returns `404 Not Found` when the token is not configured or after any administrator
exists. Remove `Bootstrap__Token` from the environment immediately after setup. The
token and password are never written to the audit log.

## Test and coverage

```sh
dotnet test dashboard/Aura.Dashboard.Tests --collect:"XPlat Code Coverage"
```

## GitHub Actions deployment

`.github/workflows/dashboard.yml` runs separate build and test jobs for pull requests
and pushes that change the dashboard. After both jobs succeed on `main`, its deploy
job publishes the release artifact to Azure Web App.

Configure these repository Actions values:

- Variable `AZURE_DASHBOARD_APP_NAME`: the Azure Web App resource name.
- Secret `AZURE_DASHBOARD_PUBLISH_PROFILE`: the complete publish-profile XML
  downloaded from that Web App.

Configure `ConnectionStrings__MongoDb`, `Mongo__Database`, storage settings, and any
one-time `Bootstrap__Token` as Azure Web App application settings rather than GitHub
build variables. Remove the bootstrap token after creating the first administrator.
Manual runs are available through **Actions → Management Dashboard → Run workflow**;
deployment still occurs only when the run targets `main`.
