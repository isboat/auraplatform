# Management Dashboard

## Overview

The Aura Management Dashboard is a .NET MVC application for platform administration and content moderation. It is separate from the public React frontend and is available only to authenticated staff members with the **Content Reviewer** or **Administrator** role.

The dashboard provides secure workflows for reviewing uploaded media, recording moderation decisions, managing users, removing media, and monitoring high-level platform activity.

## Goals

- Give content reviewers a focused queue for pending uploads.
- Allow reviewers to approve or reject an upload and record the reason for the decision.
- Maintain an immutable audit history of moderation and administrative actions.
- Give administrators tools to manage user accounts and media.
- Present useful platform totals, including users and media assets by type.
- Enforce role-based access in both the MVC user interface and its backend actions.

## Application Architecture

The management dashboard is built as a .NET MVC application. It should follow a controller → service → repository architecture and SOLID principles:

- **Controllers** handle MVC routes, authorization policies, model validation, and view selection.
- **Services** implement moderation, user-management, reporting, and audit rules.
- **Repositories** isolate MongoDB persistence and queries.
- **Views and view models** present only the information required for each management task.
- **Infrastructure adapters** provide authentication, password-reset delivery, logging, and access to configured media storage.

The dashboard should reuse shared domain contracts where practical, while keeping its administrative workflows and permissions isolated from the public application.

## Roles and Permissions

There are two management roles.

### Content Reviewer

A Content Reviewer can:

- Sign in to the management dashboard.
- View uploads awaiting review.
- Open an upload and inspect its title, description, tags, owner, type, upload date, and media preview.
- Approve an upload.
- Reject an upload with a required comment or reason.
- View audit records relevant to content review.

A Content Reviewer cannot manage user accounts, reset passwords, or perform administrator-only platform operations.

### Administrator

An Administrator has all Content Reviewer permissions and can also:

- View and search user accounts.
- Edit permitted user-account details.
- Delete or block a user account.
- Unblock a blocked user account.
- Initiate a secure password reset for a user.
- Delete media from the platform and its configured storage provider.
- View platform totals and reporting summaries.
- View the complete audit log.

### Permission Matrix

| Capability | Content Reviewer | Administrator |
| --- | :---: | :---: |
| Sign in to the dashboard | Yes | Yes |
| View the review queue | Yes | Yes |
| Preview an upload | Yes | Yes |
| Approve an upload | Yes | Yes |
| Reject an upload with a reason | Yes | Yes |
| View moderation audit history | Yes | Yes |
| View and search users | No | Yes |
| Edit users | No | Yes |
| Block, unblock, or delete users | No | Yes |
| Initiate password resets | No | Yes |
| Delete media | No | Yes |
| View platform-wide metrics | No | Yes |
| View the complete audit log | No | Yes |

## Authentication and Security

- Staff must authenticate before accessing any dashboard page.
- Authorization must be enforced on the server with role-based policies; hiding a navigation item is not sufficient authorization.
- Only active, non-blocked accounts assigned the Content Reviewer or Administrator role may sign in.
- Authentication cookies should be `HttpOnly`, `Secure`, and use an appropriate `SameSite` policy.
- State-changing requests must use anti-forgery protection.
- Sessions should expire after a configurable period of inactivity.
- Sign-in attempts and security-sensitive account actions should be rate-limited and audited.
- Passwords must never be displayed or sent to administrators. Password management must use a time-limited reset workflow.
- Secrets and connection strings must come from environment-specific configuration or a managed secret store.

## Dashboard Home

The landing page provides an at-a-glance operational summary.

### Content Summary

- Total number of media assets.
- Number of videos.
- Number of images.
- Number of audio assets.
- Number of uploads currently awaiting review.
- Number of approved uploads.
- Number of rejected uploads.

### User Summary

- Total number of registered users.
- Number of verified users.
- Number of blocked users.
- Number of Content Reviewers.
- Number of Administrators.

Summary cards should link to their corresponding filtered lists when the signed-in staff member has permission to view them.

## Content Review Workflow

### Review Queue

The review queue lists uploads with the `InReview` status. The newest pending uploads should appear first. Reviewers should be able to filter or search by:

- Media title.
- Uploader name or email.
- Media type: video, image, or audio.
- Tags.
- Upload date or date range.

Each result should show enough information to prioritize work, including the media title, type, uploader, upload date and time, and current status.

### Review Details

The review page should display:

1. Media title.
2. A safe media preview appropriate to the asset type.
3. Description and tags.
4. Media type, file details, uploader, and upload date and time.
5. Current review status.
6. Previous review history, if any.
7. Approve and Reject actions.

If the underlying media asset is unavailable, the page should show an appropriate message and must not silently approve it.

### Approve

When a reviewer approves an upload:

1. The dashboard asks for confirmation.
2. The reviewer may enter an optional internal comment.
3. The media status changes from `InReview` to `Approved`.
4. The media becomes available in public discovery and search.
5. An audit entry records the decision.

The service must prevent conflicting decisions if two reviewers open the same upload. A decision should succeed only when the current status is still `InReview`.

### Reject

When a reviewer rejects an upload:

1. The dashboard requires a reason or comment.
2. The reason is validated and stored with the decision.
3. The media status changes from `InReview` to `Rejected`.
4. The media remains unavailable to the public.
5. An audit entry records the decision and reason.

Rejection reasons may be shown to the uploader where product policy permits, but internal-only notes must remain visible only to authorized staff.

## User Management

User management is restricted to Administrators.

### User List and Details

Administrators can view and search users by name, email, verification state, account state, or role. The user details page should show:

- Name and email.
- Email-verification status.
- Account state: active or blocked.
- Assigned platform and management roles.
- Registration date and last relevant account activity, when available.
- Number of uploaded media assets.
- Recent administrative actions affecting the account.

### Edit User

Administrators can edit explicitly permitted profile or role fields. Changes require validation, confirmation for privilege changes, and an audit record containing the changed fields without recording sensitive values.

### Block and Unblock User

Blocking a user prevents new sign-ins and invalidates or rejects active access as soon as practical. Blocking does not automatically delete the user's media unless platform policy explicitly requires it. The administrator must provide a reason, and both block and unblock actions are audited.

### Delete User

Deleting an account is a destructive operation requiring confirmation. The implementation must define and display how owned uploads, comments, reactions, and audit references are handled before deletion. Audit records must retain a stable subject identifier even if the user account is later removed.

### Reset Password

Administrators initiate a password reset rather than choosing or viewing a user's password. The platform sends the user a one-time, expiring reset link and records that the reset was requested. Reset tokens and passwords must never be written to the audit log.

## Media Management

Administrators can search and inspect media in any review state. They can delete a media record and its asset from the currently configured Amazon S3 or Azure Blob Storage provider.

Media deletion must:

- Require explicit confirmation.
- Verify that the media still exists and identify its owner.
- Remove or consistently retain associated comments according to platform policy.
- Attempt deletion from object storage through the shared media-storage abstraction.
- Record the media identifier, title, owner, reason, actor, and outcome in the audit log.
- Surface partial failures for retry rather than reporting a successful deletion when storage cleanup failed.

## Audit Log

The audit log provides an accountable history of moderation and administrative activity. Audit entries are append-only and must not be editable through the dashboard.

Each audit entry should include:

- A unique audit-event identifier.
- Event type, such as `MediaApproved`, `MediaRejected`, `MediaDeleted`, `UserEdited`, `UserBlocked`, `UserUnblocked`, `UserDeleted`, or `PasswordResetRequested`.
- UTC date and time.
- Staff actor identifier, name, email, and role at the time of the action.
- Target type and stable target identifier.
- Media title or user display information needed to understand the event.
- Previous and new status for state transitions.
- Review comment, rejection reason, or administrative reason when applicable.
- Request correlation identifier.
- Outcome: succeeded or failed.
- Non-sensitive change details useful for investigation.

The audit page should support filtering by event type, actor, target, outcome, and date range. Results should be ordered newest first and loaded in pages. Access to complete audit history is restricted to Administrators; Content Reviewers may view moderation records allowed by policy.

Audit records must never contain passwords, password hashes, reset tokens, JWTs, authentication cookies, storage credentials, SAS tokens, or full connection strings.

## Data and Consistency Requirements

- Store management roles and account status in MongoDB.
- Store moderation decisions and audit events in dedicated MongoDB collections.
- Store all timestamps in UTC and format them for the viewer's locale in the UI.
- Use stable identifiers so audit records remain useful after related records are changed or deleted.
- Apply optimistic concurrency or an atomic status condition when approving or rejecting uploads.
- Use pagination for users, media, review queues, and audit logs.
- Create indexes for common filters, including review status and upload date, user email and account state, and audit event date, actor, target, and type.

## Error Handling and Accessibility

- Display clear success or failure feedback after every management action.
- Preserve entered rejection reasons when a recoverable request fails.
- Disable repeated submissions while an action is processing.
- Use accessible labels, validation summaries, keyboard navigation, focus management, and sufficient color contrast.
- Do not rely on color alone to communicate approval, rejection, blocked status, or failure.
- Log unexpected errors with a correlation identifier while showing staff a safe, actionable message.

## Suggested MVC Areas

- `Account` — staff sign-in, sign-out, and access-denied pages.
- `Dashboard` — platform and review summary.
- `Reviews` — review queue, media preview, approval, rejection, and review history.
- `Users` — administrator-only user search, details, edit, block, delete, and password reset.
- `Media` — administrator-only media search, details, and deletion.
- `Audit` — role-filtered audit history and event details.

## Testing Expectations

The management application should include:

- Unit tests for moderation, account management, media deletion, reporting, and audit services.
- Authorization tests proving that Content Reviewers cannot access Administrator actions.
- Controller tests for validation, anti-forgery behavior, redirects, and status messages.
- Repository integration tests for atomic review decisions and audit persistence.
- End-to-end tests for sign-in, approve, reject-with-reason, user blocking, password-reset initiation, media deletion, metrics, and audit filtering.
- Tests confirming that concurrent reviewers cannot overwrite an existing decision.
- Tests confirming that sensitive credentials and tokens never appear in audit records.

## Definition of Done

The management dashboard is ready for release when:

- Content Reviewers and Administrators can sign in securely.
- Both roles can review, approve, and reject pending uploads according to their permissions.
- A rejection cannot be submitted without a reason.
- Every moderation and administrator action produces an audit event.
- Administrators can view, edit, block, unblock, delete, and initiate password resets for user accounts.
- Administrators can delete media through the configured storage abstraction.
- Administrators can see user totals and media totals by type and review status.
- Server-side authorization prevents Content Reviewers from using Administrator-only operations.
- Lists are searchable, filterable, and paginated.
- Automated tests cover critical permissions, workflows, audit behavior, and failure paths.
