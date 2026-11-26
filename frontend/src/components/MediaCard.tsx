import { Eye, Heart, Image, Music2, Play } from 'lucide-react';
import { useState } from 'react';
import type { Media, MediaType } from '../types';

const icons: Record<MediaType, typeof Play> = { video: Play, image: Image, audio: Music2 };
// eslint-disable-next-line react-refresh/only-export-components
export const compact = (value: number) => Intl.NumberFormat('en', { notation: 'compact', maximumFractionDigits: 1 }).format(value);

function Preview({ item }: { item: Media }) {
  const [failed, setFailed] = useState(false);
  const Icon = icons[item.mediaType] ?? Image;
  if (!item.url || failed) return <div className="media-placeholder">
    <Icon size={24} aria-hidden="true" />
    <span>{!item.url && item.reviewStatus === 'InReview' ? 'In review' : 'Preview unavailable'}</span>
  </div>;
  if (item.mediaType === 'image') return <img src={item.url} alt="" loading="lazy" onError={() => setFailed(true)} />;
  if (item.mediaType === 'video') return <video src={item.url} preload="metadata" muted playsInline aria-hidden="true" onError={() => setFailed(true)} />;
  return <div className="media-placeholder audio"><Music2 size={32} aria-hidden="true" /></div>;
}

export default function MediaCard({ item }: { item: Media }) {
  const Icon = icons[item.mediaType] ?? Image;
  return <a className="media-card" href={`/media/${item.id}`}>
    <div className="media-art"><Preview key={item.url} item={item} /><span className="type-pill"><Icon size={13} />{item.mediaType}</span><span className="play" aria-hidden="true"><Icon size={25} /></span></div>
    <div className="card-body"><h3>{item.title}</h3><p>by {item.ownerName}</p><div className="stats"><span><Eye size={14} aria-hidden="true" /><span className="visually-hidden">Views</span>{compact(item.views)}</span><span><Heart size={14} aria-hidden="true" /><span className="visually-hidden">Likes</span>{compact(item.likes)}</span></div></div>
  </a>;
}
