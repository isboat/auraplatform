# Memory caching assessment

Both server applications can benefit from process-local memory caching, but only for
small, frequently read values for which short-lived staleness is acceptable. An
in-memory cache is private to one application instance; it is not a replacement for
MongoDB and it does not coordinate invalidation across scaled-out API or dashboard
instances.

## Backend API

The platform feature flags are read for registration, upload, and configuration API
requests, but they change infrequently. The API caches `PlatformConfiguration` for
one minute and immediately replaces the local cached value after a successful update.
Registration and upload services use the configuration service so these requests
share the cached read. Changes made outside an API instance can take up to one minute
to become visible to that instance.

User records, JWT session versions, media details, reactions, view counts, comments,
and signed media URLs are intentionally not cached. Those values either participate
in authentication/authorization, change often, or must reflect a write immediately.
In particular, caching the session-version lookup could allow a revoked session to
remain valid.

## Management dashboard

The dashboard summary performs aggregate media and user count queries on every page
load. These read-only metrics tolerate a small delay, so the combined result is cached
for 15 seconds. The short absolute lifetime bounds staleness from writes made by this
dashboard, the public API, or another dashboard instance.

Review queues, user search results, media/user details, authentication checks, and
bootstrap state are intentionally not cached. They drive moderation, account access,
and concurrency decisions and therefore need current database state.

## Scaling considerations

The current approach is useful for a single instance and still reduces repeated reads
within each instance when the applications scale out. If strict cross-instance cache
coherence or longer lifetimes become necessary, use a distributed cache and explicit
invalidation events. Cache hit rate, MongoDB query latency, and acceptable staleness
should be measured before expanding caching to additional data.
