import { ArrowUpRight } from 'lucide-react';
import type { Media } from '../types';
import MediaCard from './MediaCard';
import { EmptyState, MediaSkeleton } from './States';

export default function MediaSection({ title, items }: { title: string; items: Media[] | null }) {
  return <section className="media-section">
    <div className="section-heading">
      <h2>{title}</h2>
      {!!items?.length && <a href="/search">Browse media <ArrowUpRight size={15} aria-hidden="true" /></a>}
    </div>
    {items === null ? <MediaSkeleton /> : items.length
      ? <div className="media-grid">{items.map(item => <MediaCard key={item.id} item={item} />)}</div>
      : <EmptyState title="Nothing to show yet">Approved media will appear here.</EmptyState>}
  </section>;
}
