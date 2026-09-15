import { useState, type FormEvent } from 'react';
import { CheckCircle2, FileVideo, LoaderCircle, UploadCloud, X } from 'lucide-react';
import { api } from '../api';

export default function UploadPage() {
  const [file, setFile] = useState<File | null>(null);
  const [done, setDone] = useState(false);
  const [description, setDescription] = useState('');
  const [progress, setProgress] = useState(0);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!file || uploading) return;

    setError('');
    setProgress(0);
    setUploading(true);
    const data = new FormData(event.currentTarget);

    try {
      await api.upload(
        file,
        String(data.get('title')),
        description,
        String(data.get('tags')).split(',').map(tag => tag.trim()).filter(Boolean),
        setProgress
      );
      setDone(true);
    } catch (reason) {
      setError((reason as Error).message);
      setUploading(false);
      setProgress(0);
    }
  };

  if (done) {
    return <div className="center-card success">
      <CheckCircle2 />
      <h1>Your media is under review.</h1>
      <p>We’ll make it available as soon as the review is complete. Follow its status from My Uploads.</p>
      <a className="button primary" href="/my-uploads">View my uploads</a>
    </div>;
  }

  const uploadLabel = progress === 0
    ? 'Preparing upload…'
    : progress < 100
      ? `Uploading… ${progress}%`
      : 'Finalizing upload…';

  return <div className="page-narrow">
    <div className="page-heading">
      <span className="eyebrow">Creator studio</span>
      <h1>Share something remarkable.</h1>
      <p>Video, image, or audio — bring your work to Aura.</p>
    </div>
    <form className="upload-form" onSubmit={submit} aria-busy={uploading}>
      <label className="dropzone">
        <input type="file" accept="video/*,image/*,audio/*" onChange={event => setFile(event.target.files?.[0] ?? null)} required disabled={uploading} />
        {file
          ? <><FileVideo /><strong>{file.name}</strong><span>{(file.size / 1048576).toFixed(1)} MB</span></>
          : <><UploadCloud /><strong>Drop your media here</strong><span>or click to browse · Large files upload in resilient parts</span></>}
      </label>
      {file && !uploading && <button type="button" className="remove-file" onClick={() => setFile(null)}><X />Remove</button>}
      <div className="form-grid">
        <label>Title <span>Optional</span><input name="title" placeholder="Untitled masterpiece" disabled={uploading} /></label>
        <label className="full">Description <span>{description.length}/255</span><textarea name="description" maxLength={255} value={description} onChange={event => setDescription(event.target.value)} placeholder="Tell people about this media…" disabled={uploading} /></label>
        <label className="full">Tags <span>Separate with commas</span><input name="tags" placeholder="travel, ambient, documentary" disabled={uploading} /></label>
      </div>
      <div className="responsibility">
        <strong>You are responsible for the media you upload.</strong>
        <span>Only share work you own or have permission to use.</span>
      </div>
      {error && <p className="inline-error">{error}</p>}
      {uploading && <div className="upload-progress" role="status" aria-live="polite">
        <div className="upload-progress-heading"><span>{uploadLabel}</span><strong>{progress}%</strong></div>
        <div className="progress" role="progressbar" aria-label="Media upload progress" aria-valuemin={0} aria-valuemax={100} aria-valuenow={progress}>
          <span style={{ width: `${progress}%` }} />
        </div>
        <small>Please keep this page open while your media is being uploaded.</small>
      </div>}
      <button className="button primary submit-upload" disabled={!file || uploading}>
        {uploading ? <><LoaderCircle className="upload-spinner" aria-hidden="true" />{uploadLabel}</> : <>Upload for review <UploadCloud /></>}
      </button>
    </form>
  </div>;
}
