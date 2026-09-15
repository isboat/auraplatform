import { useState, type FormEvent } from 'react';
import { ArrowRight, CheckCircle2 } from 'lucide-react';
import { api } from '../api';

export default function PasswordResetPage({ reset = false }: { reset?: boolean }) {
  const [complete, setComplete] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const token = new URLSearchParams(location.search).get('token') ?? '';

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setBusy(true);
    setError('');
    const data = new FormData(event.currentTarget);
    try {
      const response = reset
        ? await api.resetPassword(token, String(data.get('password')), String(data.get('confirmPassword')))
        : await api.forgotPassword(String(data.get('email')));
      setMessage(response.message);
      setComplete(true);
    } catch (reason) {
      setError((reason as Error).message);
    } finally {
      setBusy(false);
    }
  };

  if (reset && !token) return <div className="center-card"><h1>Invalid reset link</h1><p>This password reset link is incomplete. Request a new link and try again.</p><a className="button primary" href="/forgot-password">Request a new link</a></div>;
  if (complete) return <div className="center-card success"><CheckCircle2 /><h1>{reset ? 'Password reset' : 'Check your inbox'}</h1><p>{message}</p><a className="button primary" href="/signin">Continue to sign in</a></div>;

  return <div className="auth-wrap"><div className="auth-aside"><span className="eyebrow">Account recovery</span><h1>{reset ? 'Choose a new password.' : 'Let’s get you back in.'}</h1><p>{reset ? 'Use a strong password you do not use elsewhere.' : 'We’ll send a secure, time-limited reset link if the email is eligible.'}</p></div><form className="auth-form" onSubmit={submit}><span className="eyebrow">Password help</span><h2>{reset ? 'Reset your password' : 'Forgot your password?'}</h2>{error && <p className="inline-error" role="alert">{error}</p>}{reset ? <><label>New password<input name="password" type="password" minLength={8} autoComplete="new-password" required placeholder="At least 8 characters" /></label><label>Confirm new password<input name="confirmPassword" type="password" minLength={8} autoComplete="new-password" required placeholder="Enter it again" /></label></> : <label>Email address<input name="email" type="email" autoComplete="email" required placeholder="you@example.com" /></label>}<button className="button primary" disabled={busy}>{busy ? 'Please wait…' : reset ? 'Reset password' : 'Send reset link'}<ArrowRight size={17} /></button><p><a href="/signin">Back to sign in</a></p></form></div>;
}
