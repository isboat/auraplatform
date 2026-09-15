# Project Overview

Aura Platform is a media hosting platform where users can upload and share videos, images, and audio. The platform makes hosted media available to both anonymous visitors and signed-in users.

## Media

Aura Platform supports the following media types:

- Videos
- Images
- Audio

All users can browse and interact with the hosted media in the appropriate format: they can view images, listen to audio, and watch videos.

## Homepage

The homepage organizes media into multiple discovery sections. Each section displays up to 10 items, including:

- Latest approved media
- Most-viewed media
- Most-liked media

The latest-media section includes only uploads that platform administrators have approved; media still awaiting review is excluded.

Additional discovery sections can follow the same limited, ranked format as the platform evolves.

## Uploads and Content Discovery

The upload page allows users to provide the following information for each video, image, or audio upload:

- Title (optional)
- Description (maximum 255 characters)
- Tags

If a title is not provided, the backend generates one from the current date and time. The time portion uses the `hh mm ss` format.

Tags make uploaded content discoverable through search. Users can search by tag to find matching videos, images, or audio.

The upload page displays a responsibility notice: **You are responsible for the media you upload.**

## Media Storage and Large Uploads

Media assets are stored as objects in an Amazon S3 bucket. The upload workflow is designed to transfer videos, images, and audio efficiently and reliably without routing entire large files through application memory.

Large files use multipart uploads so that file parts can be uploaded independently. If a transfer is interrupted, the platform can retry failed parts instead of restarting the complete upload.

## Content Review

Every upload is submitted for review by platform administrators. While an upload is awaiting review, users can still find it through search, but the media is covered by an **In review** status message.

After an upload completes successfully, the platform shows the uploader a confirmation message: **Your media is under review.**

## Account Registration and Verification

Users can create an account by completing a registration form with:

- Name
- Email address, used as the username
- Password

After registration, the platform sends a verification email containing a link to the email verification page. The user follows the link to verify their email address, and successful verification completes the account setup.

## Sign In and Authentication

The platform provides a sign-in page where users enter:

- Their email address as the username
- Their password

After a successful sign-in, the backend issues a JSON Web Token (JWT). The client includes this token with subsequent requests that require an authenticated user, allowing the backend to authorize access to protected platform features.

## Administrative Configuration

Administrators can independently turn off:

- The signup and registration page
- The media upload page

These feature settings are stored in MongoDB configuration collections. The platform reads the current settings and adapts the experience accordingly: disabled pages and their related navigation or actions are not rendered, while the backend also enforces each setting so users cannot bypass it by calling an API directly.

## User Access

### Anonymous Users

Anonymous users can access hosted media without signing in. They can view images, listen to audio, and watch videos.

### Signed-In Users

Signed-in users have the same access to hosted media as anonymous users. They can also:

- Leave comments in a media item's comments section
- Like or dislike media
- Share media
- Delete media they uploaded

## My Uploads

Signed-in users have a **My Uploads** page that lists all media they have uploaded. Each upload displays:

- Number of views
- Number of likes
- Number of dislikes
- Number of comments

Each item also provides an option for the user to delete that upload.

## Direct Content Links

Each media item has a direct content-page URL. When a user pastes or opens that URL in a browser, the platform resolves the route and takes the user directly to the corresponding video, image, or audio page instead of redirecting them to the platform's home page.

## Content Page Layout

Each content page presents its information in the following order:

1. Title
2. Video, image, or audio media
3. Engagement summary
4. Description
5. Comments section

## Views and Engagement

The platform keeps track of the number of views for each video, image, and audio item. An engagement summary displayed below the content shows:

- Number of likes with a thumbs-up icon
- Number of dislikes with a thumbs-down icon
- Number of views

This layout gives users a familiar, YouTube-style overview of the content's engagement.

## Comments

To improve content-page load performance, the comments section is hidden initially and comments are not requested with the initial page data. The user can select **Show comments** to reveal the section and load the first batch.

Signed-in users can add comments from this section. Comments are ordered newest first so the latest contribution appears at the top.

Each displayed comment identifies the user who submitted it and shows the date and time it was added.

Comments are retrieved in batches rather than all at once. Additional batches load incrementally as the user continues through the comments, reducing the initial data transfer and rendering work for media with long discussions.

## Advertising

Aura Platform displays advertisements as part of its media hosting experience. Ads appear on the platform alongside hosted media and its related features.

## Static Legal Pages

The React frontend includes the following static pages:

- Privacy
- Terms of Use

These pages are rendered from frontend content and do not require content to be fetched from the backend API.

## Design and Styling

Aura Platform uses modern design and styling to provide a contemporary experience throughout the platform. Its visual presentation supports browsing, viewing, listening to, and interacting with hosted media.

The interface is responsive across mobile phones, tablets, and laptops. Layouts and media experiences adapt to each device's screen size so that platform features remain accessible and usable.

## Technology Stack

- **Backend:** C# with an ASP.NET Core Web API targeting .NET 10
- **Frontend:** The latest stable version of React
- **Database:** MongoDB
- **Media storage:** Amazon S3 or Azure Blob Storage, selected through backend configuration

## Purpose

The platform provides a central place for hosting different types of media while allowing the public to consume that content. User accounts add community and sharing features without restricting anonymous access to the hosted media.

Administrative and content-review workflows are defined separately in the [Management Dashboard](management-dashboard.md) requirements.
