import { useEffect, useState } from 'react';
import { Eye, ThumbsUp, ThumbsDown, MessageCircle, Trash2 } from 'lucide-react';
import { api } from '../api';
import { compact } from '../components/MediaCard';
import { ErrorState, LoadingState } from '../components/States';
import type { Media } from '../types';

export default function MyUploadsPage() {
  const [items, setItems] = useState<Media[] | null>(null); const [error, setError] = useState('');
  useEffect(() => { api.myUploads().then(setItems).catch((reason: Error) => setError(reason.message)); }, []);
  const remove = async (item: Media) => { if (!confirm(`Delete “${item.title}”? This cannot be undone.`)) return; try { await api.deleteMedia(item.id); setItems(current => current?.filter(value => value.id !== item.id) ?? []); } catch (reason) { setError((reason as Error).message); } };
  if (error && !items) return <ErrorState title="Uploads unavailable" message={error} />; if (!items) return <LoadingState label="Loading your uploads…" />;
  return <div className="page-wide"><div className="page-heading inline"><div><span className="eyebrow">Creator studio</span><h1>My uploads</h1><p>Track performance and manage your media.</p></div><a className="button primary" href="/upload">Upload media</a></div>{error && <p className="inline-error">{error}</p>}{items.length ? <div className="uploads-table">{items.map(item => <div className="upload-row" key={item.id}><div className="upload-thumb">{item.url && item.mediaType === 'image' ? <img src={item.url} alt="" /> : <FileType type={item.mediaType} />}</div><div className="upload-info"><span className="status">{item.reviewStatus === 'InReview' ? 'In review' : item.reviewStatus}</span><a href={`/media/${item.id}`}>{item.title}</a><small>{new Date(item.createdAt).toLocaleDateString()}</small></div><div className="upload-metrics"><span><Eye />{compact(item.views)}<small>Views</small></span><span><ThumbsUp />{compact(item.likes)}<small>Likes</small></span><span><ThumbsDown />{item.dislikes}<small>Dislikes</small></span><span><MessageCircle />{item.comments}<small>Comments</small></span></div><button className="delete" onClick={() => remove(item)} aria-label={`Delete ${item.title}`}><Trash2 /></button></div>)}</div> : <p className="empty">You have not uploaded any media yet.</p>}</div>;
}
function FileType({ type }: { type: string }) { return <span className="file-type">{type}</span>; }
