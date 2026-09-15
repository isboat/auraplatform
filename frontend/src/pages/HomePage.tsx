import { useEffect, useState } from 'react';
import { ArrowRight, Sparkles } from 'lucide-react';
import { api } from '../api';
import MediaSection from '../components/MediaSection';
import { ErrorState, LoadingState } from '../components/States';
import type { HomepageMedia } from '../types';

export default function HomePage() {
  const [data, setData] = useState<HomepageMedia | null>(null);
  const [error, setError] = useState('');
  useEffect(() => { api.home().then(setData).catch((reason: Error) => setError(reason.message)); }, []);
  if (error) return <ErrorState title="Aura is unavailable" message={error} />;
  if (!data) return <LoadingState label="Finding remarkable media…" />;
  return <><section className="hero"><div className="hero-copy"><span className="badge"><Sparkles size={14} />Curated for curious minds</span><h1>See it. Hear it.<br /><em>Feel it.</em></h1><p>Discover remarkable videos, images, and sounds from creators around the world.</p><a className="button primary" href="#discover">Start exploring <ArrowRight size={17} /></a></div><div className="hero-collage"><div className="collage-main aurora"><span className="floating-label">Featured story<br /><strong>Discover something new</strong></span></div><div className="collage-small desert" /><div className="sound-card"><i /><i /><i /><i /><i /><span>Listen differently</span></div></div></section><div id="discover"><MediaSection kicker="Fresh today" title="Latest approved" items={data.latest} /><MediaSection kicker="What everyone's watching" title="Most viewed" items={data.mostViewed} /><MediaSection kicker="Community favorites" title="Most liked" items={data.mostLiked} /></div></>;
}
