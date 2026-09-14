import type { AuthResponse, Comment, HomepageMedia, Media, MessageResponse, PlatformConfiguration, UploadStart } from './types';

const API_URL = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? 'http://localhost:5080/api';
const PART_SIZE = 10 * 1024 * 1024;

export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message); }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('aura-token');
  const headers = new Headers(options.headers);
  if (!(options.body instanceof Blob) && options.body !== undefined) headers.set('Content-Type', 'application/json');
  if (token) headers.set('Authorization', `Bearer ${token}`);
  const response = await fetch(`${API_URL}${path}`, { ...options, headers });
  if (!response.ok) {
    const problem = await response.json().catch(() => null) as { detail?: string; message?: string; title?: string } | null;
    throw new ApiError(response.status, problem?.detail ?? problem?.message ?? problem?.title ?? 'The request could not be completed.');
  }
  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export const api = {
  configuration: () => request<PlatformConfiguration>('/configuration'),
  updateConfiguration: (value: Pick<PlatformConfiguration, 'registrationEnabled' | 'uploadsEnabled'>) => request<PlatformConfiguration>('/configuration', { method: 'PUT', body: JSON.stringify(value) }),
  register: (name: string, email: string, password: string) => request<MessageResponse>('/auth/register', { method: 'POST', body: JSON.stringify({ name, email, password }) }),
  login: (email: string, password: string) => request<AuthResponse>('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  home: () => request<HomepageMedia>('/media/home'),
  search: (tag: string) => request<Media[]>(`/media/search?tag=${encodeURIComponent(tag)}`),
  media: (id: string) => request<Media>(`/media/${encodeURIComponent(id)}`),
  myUploads: () => request<Media[]>('/media/mine'),
  deleteMedia: (id: string) => request<void>(`/media/${encodeURIComponent(id)}`, { method: 'DELETE' }),
  react: (id: string, like: boolean) => request<void>(`/media/${encodeURIComponent(id)}/reaction`, { method: 'POST', body: JSON.stringify({ like }) }),
  comments: (id: string, before?: string) => request<Comment[]>(`/media/${encodeURIComponent(id)}/comments?limit=10${before ? `&before=${encodeURIComponent(before)}` : ''}`),
  addComment: (id: string, body: string) => request<Comment>(`/media/${encodeURIComponent(id)}/comments`, { method: 'POST', body: JSON.stringify({ body }) }),
  async upload(file: File, title: string, description: string, tags: string[], onProgress: (progress: number) => void): Promise<MessageResponse> {
    const started = await request<UploadStart>('/media/uploads', { method: 'POST', body: JSON.stringify({ title: title || null, description, tags, fileName: file.name, contentType: file.type, fileSize: file.size }) });
    const parts: { partNumber: number; eTag: string }[] = [];
    for (let index = 0; index < started.partUrls.length; index += 1) {
      const chunk = file.slice(index * PART_SIZE, Math.min(file.size, (index + 1) * PART_SIZE));
      const response = await fetch(started.partUrls[index], { method: 'PUT', body: chunk });
      if (!response.ok) throw new ApiError(response.status, `Upload part ${index + 1} failed.`);
      parts.push({ partNumber: index + 1, eTag: response.headers.get('ETag')?.replaceAll('"', '') ?? '' });
      onProgress(Math.round(((index + 1) / started.partUrls.length) * 100));
    }
    return request<MessageResponse>(`/media/${started.mediaId}/complete`, { method: 'POST', body: JSON.stringify({ uploadId: started.uploadId, parts }) });
  }
};
