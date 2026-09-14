# Project Overview

Aura Platform is a media hosting platform where users can upload and share videos, images, and audio. The platform makes hosted media available to both anonymous visitors and signed-in users.

## Media

Aura Platform supports the following media types:

- Videos
- Images
- Audio

All users can browse and interact with the hosted media in the appropriate format: they can view images, listen to audio, and watch videos.

## Uploads and Content Discovery

The upload page allows users to provide the following information for each video, image, or audio upload:

- Title (optional)
- Description
- Tags

If a title is not provided, the backend generates one from the current date and time. The time portion uses the `hh mm ss` format.

Tags make uploaded content discoverable through search. Users can search by tag to find matching videos, images, or audio.

## User Access

### Anonymous Users

Anonymous users can access hosted media without signing in. They can view images, listen to audio, and watch videos.

### Signed-In Users

Signed-in users have the same access to hosted media as anonymous users. They can also engage with media by:

- Leaving comments in a media item's comments section
- Liking or disliking media
- Sharing media

## Views and Engagement

The platform keeps track of the number of views for each video, image, and audio item. An engagement summary displayed below the content shows:

- Number of likes with a thumbs-up icon
- Number of dislikes with a thumbs-down icon
- Number of views

This layout gives users a familiar, YouTube-style overview of the content's engagement.

## Advertising

Aura Platform displays advertisements as part of its media hosting experience. Ads appear on the platform alongside hosted media and its related features.

## Design and Styling

Aura Platform uses modern design and styling to provide a contemporary experience throughout the platform. Its visual presentation supports browsing, viewing, listening to, and interacting with hosted media.

## Technology Stack

- **Backend:** C# with an ASP.NET Core Web API
- **Frontend:** React
- **Database:** MongoDB

## Purpose

The platform provides a central place for hosting different types of media while allowing the public to consume that content. User accounts add community and sharing features without restricting anonymous access to the hosted media.
