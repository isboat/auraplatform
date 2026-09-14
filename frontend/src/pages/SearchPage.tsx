import { useEffect, useState } from 'react';
import { Search } from 'lucide-react';
import { api } from '../api';
import MediaCard from '../components/MediaCard';
import { LoadingState } from '../components/States';
import type { Media } from '../types';

export default function SearchPage() {
  const query = new URLSearchParams(location.search).get('tag') ?? ''; const [items, setItems] = useState<Media[] | null>(null); const [error, setError] = useState('');
  useEffect(() => { api.search(query).then(setItems).catch((reason: Error) => setError(reason.message)); }, [query]);
  return <div className="page-wide"><div className="page-heading"><span className="eyebrow">Explore Aura</span><h1>{query ? `Results for “${query}”` : 'Search all media'}</h1><form className="big-search"><Search /><input name="tag" defaultValue={query} placeholder="Try nature, music, design…" /><button>Search</button></form></div>{error && <p className="inline-error">{error}</p>}{!items ? <LoadingState label="Searching media…" /> : items.length ? <div className="media-grid search-results">{items.map(item => <MediaCard key={item.id} item={item} />)}</div> : <p className="empty">No media matches that search yet.</p>}</div>;
}
