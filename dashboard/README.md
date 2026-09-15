# Aura Management Dashboard

The dashboard is an isolated ASP.NET Core MVC staff application implementing the workflows in `doc/management-dashboard.md`.

## Run

1. Install the .NET 10 SDK and start MongoDB.
2. Supply `ConnectionStrings__MongoDb`, `Mongo__Database`, and media-storage configuration through environment-specific configuration or a secret store.
3. Run `dotnet run --project dashboard/Aura.Dashboard`.

Only verified, active users with `ContentReviewer` or `Administrator` access can authenticate. Production deployments must replace the development storage/reset adapters with adapters for the same provider and mail delivery used by the API.

Dashboard identities are stored in the dedicated MongoDB `staffuser` collection, not
the public API's `users` collection. At application startup the dashboard ensures the
collection exists by creating a unique email index and a management state/roles index.

## Test and coverage

```sh
dotnet test dashboard/Aura.Dashboard.Tests --collect:"XPlat Code Coverage"
```
