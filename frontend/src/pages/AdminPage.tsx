import { useEffect, useState, type FormEvent } from 'react';
import { Settings, Save } from 'lucide-react';
import { api } from '../api';

export default function AdminPage() {
  const [registration, setRegistration] = useState(true); const [uploads, setUploads] = useState(true); const [saved, setSaved] = useState(false); const [error, setError] = useState('');
  useEffect(() => { api.configuration().then(value => { setRegistration(value.registrationEnabled); setUploads(value.uploadsEnabled); }).catch((reason: Error) => setError(reason.message)); }, []);
  const submit = async (event: FormEvent) => { event.preventDefault(); setError(''); try { await api.updateConfiguration({ registrationEnabled: registration, uploadsEnabled: uploads }); setSaved(true); } catch (reason) { setError((reason as Error).message); } };
  return <div className="page-narrow"><div className="page-heading"><span className="eyebrow">Administration</span><h1>Platform controls</h1><p>Control which creator features are currently available.</p></div><form className="admin-card" onSubmit={submit}><Settings /><label><span><strong>Account registration</strong><small>Allow new users to access the signup page.</small></span><input type="checkbox" checked={registration} onChange={event => { setSaved(false); setRegistration(event.target.checked); }} /></label><label><span><strong>Media uploads</strong><small>Allow signed-in users to access the upload page.</small></span><input type="checkbox" checked={uploads} onChange={event => { setSaved(false); setUploads(event.target.checked); }} /></label><button className="button primary"><Save size={17} />Save configuration</button>{saved && <p className="saved">Configuration saved.</p>}{error && <p className="inline-error">{error}</p>}</form></div>;
}
