import { ArrowRight } from 'lucide-react';
import type { Media } from '../types';
import MediaCard from './MediaCard';

export default function MediaSection({ title, kicker, items }: { title: string; kicker: string; items: Media[] }) {
  return <section className="media-section"><div className="section-heading"><div><span className="eyebrow">{kicker}</span><h2>{title}</h2></div><a href="/search">View all <ArrowRight size={16} /></a></div>
    {items.length ? <div className="media-grid">{items.map(item => <MediaCard key={item.id} item={item} />)}</div> : <p className="section-empty">No approved media is available in this section yet.</p>}
  </section>;
}
