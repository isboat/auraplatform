import { AlertCircle, Images, LoaderCircle } from 'lucide-react';
import type { ReactNode } from 'react';

export function LoadingState({ label = 'Loading media…' }: { label?: string }) {
  return <div className="center-card state" role="status">
    <LoaderCircle className="spinner" aria-hidden="true" /><p>{label}</p>
  </div>;
}

export function ErrorState({ title, message }: { title: string; message: string }) {
  return <div className="center-card state">
    <AlertCircle aria-hidden="true" /><h1>{title}</h1><p>{message}</p>
    <a className="button ghost" href="/">Return home</a>
  </div>;
}

export function EmptyState({ title, children, action }: {
  title: string;
  children: ReactNode;
  action?: ReactNode;
}) {
  return <div className="empty-state">
    <span className="state-icon"><Images size={20} aria-hidden="true" /></span>
    <div><h3>{title}</h3><p>{children}</p></div>
    {action}
  </div>;
}

export function MediaSkeleton({ label = 'Loading media…' }: { label?: string }) {
  return <div role="status">
    <span className="visually-hidden">{label}</span>
    <div className="media-grid" aria-hidden="true">
      {Array.from({ length: 4 }, (_, index) => <div className="skeleton-card" key={index}>
        <div className="skeleton skeleton-art" />
        <div className="skeleton-copy"><div className="skeleton" /><div className="skeleton" /></div>
      </div>)}
    </div>
  </div>;
}
