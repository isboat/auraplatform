import { ArrowUpRight, AudioLines, Clapperboard, Image } from 'lucide-react';
import { useEffect, useState } from 'react';
import { api } from '../api';
import MediaSection from '../components/MediaSection';
import { ErrorState } from '../components/States';
import type { HomepageMedia } from '../types';

export default function HomePage() {
  const [data, setData] = useState<HomepageMedia | null>(null);
  const [error, setError] = useState('');
  useEffect(() => { api.home().then(setData).catch((reason: Error) => setError(reason.message)); }, []);

  if (error) return <ErrorState title="Aura is unavailable" message={error} />;

  return <div className="discover-page">
    <section className="discovery-hero" aria-labelledby="discovery-title">
      <div className="hero-copy">
        <h1 id="discovery-title">Discover media from independent creators.</h1>
        <p>Explore videos, images, and audio shared by the Aura community.</p>
        <a className="button primary" href="/search">Browse media <ArrowUpRight size={17} aria-hidden="true" /></a>
      </div>
      <div className="hero-art" aria-hidden="true">
        <div className="hero-tile hero-tile-video"><Clapperboard /></div>
        <div className="hero-tile hero-tile-audio"><AudioLines /></div>
        <div className="hero-tile hero-tile-image"><Image /></div>
      </div>
    </section>
    <div id="discover">
      <MediaSection title="Latest approved" items={data?.latest ?? null} />
      {!!data?.mostViewed.length && <MediaSection title="Most viewed" items={data.mostViewed} />}
      {!!data?.mostLiked.length && <MediaSection title="Most liked" items={data.mostLiked} />}
    </div>
  </div>;
}
