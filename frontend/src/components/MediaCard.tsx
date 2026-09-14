import { Eye, Heart, Image, Music2, Play } from 'lucide-react';
import type { Media, MediaType } from '../types';

const icons: Record<MediaType, typeof Play> = { video: Play, image: Image, audio: Music2 };
// eslint-disable-next-line react-refresh/only-export-components
export const compact = (value: number) => Intl.NumberFormat('en', { notation: 'compact', maximumFractionDigits: 1 }).format(value);

function Preview({ item }: { item: Media }) {
  if (!item.url) return <div className={`media-placeholder ${item.mediaType}`}><span>In review</span></div>;
  if (item.mediaType === 'image') return <img src={item.url} alt="" loading="lazy" />;
  if (item.mediaType === 'video') return <video src={item.url} preload="none" muted aria-label={`${item.title} preview`} />;
  return <div className="media-placeholder audio"><Music2 size={46} /><span>Audio</span></div>;
}

export default function MediaCard({ item }: { item: Media }) {
  const Icon = icons[item.mediaType] ?? Image;
  return <a className="media-card" href={`/media/${item.id}`}>
    <div className="media-art"><Preview item={item} /><span className="type-pill"><Icon size={13} />{item.mediaType}</span><span className="play"><Icon size={25} /></span></div>
    <div className="card-body"><span className="eyebrow">{item.tags[0] ?? item.mediaType}</span><h3>{item.title}</h3><p>by {item.ownerName}</p><div className="stats"><span><Eye size={14} />{compact(item.views)}</span><span><Heart size={14} />{compact(item.likes)}</span></div></div>
  </a>;
}
