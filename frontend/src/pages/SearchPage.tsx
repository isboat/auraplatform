import { useEffect, useState } from 'react';
import { Search } from 'lucide-react';
import { api } from '../api';
import MediaCard from '../components/MediaCard';
import { EmptyState, MediaSkeleton } from '../components/States';
import type { Media } from '../types';

export default function SearchPage() {
  const query = new URLSearchParams(location.search).get('tag') ?? '';
  const [items, setItems] = useState<Media[] | null>(null);
  const [error, setError] = useState('');
  useEffect(() => { api.search(query).then(setItems).catch((reason: Error) => setError(reason.message)); }, [query]);

  return <div className="page-wide">
    <div className="page-heading">
      <h1>{query ? `Results for “${query}”` : 'Browse media'}</h1>
      <form className="big-search" role="search" aria-label="Browse media">
        <Search size={18} aria-hidden="true" />
        <input type="search" name="tag" aria-label="Search by tag" defaultValue={query} placeholder="Search by tag…" />
        <button>Search</button>
      </form>
    </div>
    {error ? <p className="inline-error">{error}</p> : items === null
      ? <MediaSkeleton label="Searching media…" />
      : items.length ? <div className="media-grid search-results">
        {items.map(item => <MediaCard key={item.id} item={item} />)}
      </div> : <EmptyState title={query ? 'No matching media' : 'No media available'}
        action={query && <a className="button ghost" href="/search">Clear search</a>}>
        {query ? 'Try a different tag or browse all media.' : 'Approved media will appear here.'}
      </EmptyState>}
  </div>;
}
