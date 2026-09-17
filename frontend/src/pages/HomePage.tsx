import { useEffect, useState } from 'react';
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
  return <><section className="hero" style={{ display: 'inherit', minHeight: '10px', padding: '2px clamp(22px,7vw,110px) 26px' }}><div><h1>See it. Hear it. Feel it.</h1><p style={{ maxWidth: '740px', marginBottom: '0px' }}>Discover remarkable videos, images, and sounds from creators around the world.</p></div></section><div id="discover"><MediaSection kicker="Fresh today" title="Latest approved" items={data.latest} /><MediaSection kicker="What everyone's watching" title="Most viewed" items={data.mostViewed} /><MediaSection kicker="Community favorites" title="Most liked" items={data.mostLiked} /></div></>;
}
