import { useEffect, useState, type FormEvent } from 'react';
import { Eye, ThumbsUp, ThumbsDown, Share2, MessageCircle, Send, ImageOff } from 'lucide-react';
import { api, ApiError } from '../api';
import { compact } from '../components/MediaCard';
import { ErrorState, LoadingState } from '../components/States';
import { useAuth } from '../context/AuthContext';
import type { Comment, Media } from '../types';

function MediaPlayer({ media }: { media: Media }) {
  const [failed, setFailed] = useState(false);
  const failure = failed && <div className="media-unavailable player-error"><ImageOff /><h2>Media unavailable</h2><p>The media file does not exist or could not be loaded.</p></div>;
  if (!media.url) return <div className="media-unavailable"><ImageOff /><h2>Media unavailable</h2><p>{media.reviewStatus === 'InReview' ? 'This media is currently under review.' : 'This media file does not exist or is no longer available.'}</p></div>;
  if (media.mediaType === 'video') return <div className="player-frame"><video className="native-player" src={media.url} controls preload="none" onError={() => setFailed(true)}>Your browser does not support video playback.</video>{failure}</div>;
  if (media.mediaType === 'audio') return <div className="player-frame"><div className="audio-player"><div><strong>{media.title}</strong><div className="waveform">▂▄▇▅▃▆▂▅▇▄▂▆▃▅▇▃▆▂</div><audio src={media.url} controls preload="none" onError={() => setFailed(true)}>Your browser does not support audio playback.</audio></div></div>{failure}</div>;
  return <div className="player-frame"><img className="native-player image-player" src={media.url} alt={media.title} onError={() => setFailed(true)} />{failure}</div>;
}

export default function MediaPage({ id }: { id: string }) {
  const { user } = useAuth();
  const [media, setMedia] = useState<Media | null>(null);
  const [error, setError] = useState('');
  const [reaction, setReaction] = useState<'like' | 'dislike' | null>(null);
  const [showComments, setShowComments] = useState(false);
  const [comments, setComments] = useState<Comment[]>([]);
  const [commentsLoading, setCommentsLoading] = useState(false);
  const [body, setBody] = useState('');
  const [hasMore, setHasMore] = useState(true);
  useEffect(() => { api.media(id).then(setMedia).catch((reason: ApiError) => setError(reason.status === 404 ? 'This content could not be found.' : reason.message)); }, [id]);
  const loadComments = async () => {
    setCommentsLoading(true);
    try { const batch = await api.comments(id, comments.at(-1)?.createdAt); setComments(current => [...current, ...batch]); setHasMore(batch.length === 10); setShowComments(true); } catch (reason) { setError((reason as Error).message); } finally { setCommentsLoading(false); }
  };
  const submit = async (event: FormEvent) => { event.preventDefault(); if (!body.trim()) return; try { const comment = await api.addComment(id, body); setComments(current => [comment, ...current]); setBody(''); setMedia(current => current ? { ...current, comments: current.comments + 1 } : current); } catch (reason) { setError((reason as Error).message); } };
  const react = async (value: 'like' | 'dislike') => { if (!user) { location.href = '/signin'; return; } try { await api.react(id, value === 'like'); setReaction(value); } catch (reason) { setError((reason as Error).message); } };
  if (error && !media) return <ErrorState title="Media not found" message={error} />;
  if (!media) return <LoadingState />;
  return <article className="content-page"><div className="content-title"><span className="eyebrow">{media.mediaType} · {media.ownerName}</span><h1>{media.title}</h1></div><div className="viewer"><MediaPlayer media={media} /></div><div className="engagement"><div><span><Eye />{compact(media.views)} views</span><button className={reaction === 'like' ? 'active' : ''} onClick={() => react('like')}><ThumbsUp />{compact(media.likes + (reaction === 'like' ? 1 : 0))}</button><button className={reaction === 'dislike' ? 'active' : ''} onClick={() => react('dislike')}><ThumbsDown />{media.dislikes + (reaction === 'dislike' ? 1 : 0)}</button></div><button onClick={() => navigator.clipboard?.writeText(location.href)}><Share2 />Share</button></div>{error && <p className="inline-error">{error}</p>}<section className="description"><h2>About this media</h2><p>{media.description || 'No description was provided.'}</p><div className="tags">{media.tags.map(tag => <a href={`/search?tag=${encodeURIComponent(tag)}`} key={tag}>#{tag}</a>)}</div></section><section className="comments"><div className="comments-title"><div><span className="eyebrow">Conversation</span><h2>{media.comments} comments</h2></div>{!showComments && <button className="button ghost" disabled={commentsLoading} onClick={loadComments}><MessageCircle />{commentsLoading ? 'Loading…' : 'Show comments'}</button>}</div>{showComments && <>{user ? <form className="comment-form" onSubmit={submit}><span className="avatar">{user.name[0].toUpperCase()}</span><input value={body} onChange={event => setBody(event.target.value)} placeholder="Add to the conversation…" /><button aria-label="Post comment"><Send /></button></form> : <p className="notice"><a href="/signin">Sign in</a> to join the conversation.</p>}<div className="comment-list">{comments.map(comment => <div className="comment" key={comment.id}><span className="avatar">{comment.userName[0]}</span><div><strong>{comment.userName}</strong><time>{new Date(comment.createdAt).toLocaleString()}</time><p>{comment.body}</p></div></div>)}</div>{hasMore && <button className="button ghost load-more" disabled={commentsLoading} onClick={loadComments}>{commentsLoading ? 'Loading…' : 'Load more comments'}</button>}</>}</section></article>;
}
