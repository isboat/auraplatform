import { useEffect, useState } from 'react';
import { AlertTriangle, X } from 'lucide-react';
import { clearApiError, subscribeToApiErrors, type ApiError } from '../api';

export default function ApiErrorBanner() {
  const [error, setError] = useState<ApiError | null>(null);
  useEffect(() => subscribeToApiErrors(setError), []);
  if (!error) return null;
  return <div className="api-error-banner" role="alert">
    <AlertTriangle aria-hidden="true" />
    <div><strong>We couldn’t complete that request.</strong><span>{error.message}</span></div>
    <button type="button" onClick={clearApiError} aria-label="Dismiss error"><X /></button>
  </div>;
}
