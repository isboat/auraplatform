import { AlertCircle, LoaderCircle } from 'lucide-react';
export function LoadingState({ label = 'Loading media…' }: { label?: string }) { return <div className="center-card state"><LoaderCircle className="spinner" /><h1>{label}</h1></div>; }
export function ErrorState({ title, message }: { title: string; message: string }) { return <div className="center-card state"><AlertCircle /><h1>{title}</h1><p>{message}</p><a className="button primary" href="/">Return home</a></div>; }
