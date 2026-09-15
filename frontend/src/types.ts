export type ReviewStatus = 'InReview' | 'Approved' | 'Rejected';
export type MediaType = 'video' | 'image' | 'audio';

export interface User {
  id: string;
  name: string;
  email: string;
  isAdministrator: boolean;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface MessageResponse { message: string }

export interface PlatformConfiguration {
  id: string;
  registrationEnabled: boolean;
  uploadsEnabled: boolean;
}

export interface Media {
  id: string;
  title: string;
  description: string;
  tags: string[];
  mediaType: MediaType;
  reviewStatus: ReviewStatus;
  createdAt: string;
  ownerName: string;
  views: number;
  likes: number;
  dislikes: number;
  comments: number;
  url: string | null;
}

export interface HomepageMedia {
  latest: Media[];
  mostViewed: Media[];
  mostLiked: Media[];
}

export interface Comment {
  id: string;
  mediaId: string;
  userId: string;
  userName: string;
  body: string;
  createdAt: string;
}

export interface UploadStart {
  mediaId: string;
  uploadId: string;
  partUrls: string[];
  message: string;
}
