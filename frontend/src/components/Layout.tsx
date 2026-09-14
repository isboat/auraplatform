import { Headphones, Home, Search, Upload, UserRound, LogOut, Menu, X, Shield } from 'lucide-react';
import { useState, type ReactNode } from 'react';
import { useAuth } from '../context/AuthContext';
import { useConfiguration } from '../hooks';

export default function Layout({ children }: { children: ReactNode }) {
  const { user, signOut } = useAuth();
  const configuration = useConfiguration();
  const [open, setOpen] = useState(false);
  return <div className="app-shell">
    <header>
      <a className="brand" href="/"><span className="brand-mark"><Headphones size={20} /></span><span>aura</span></a>
      <nav className={open ? 'open' : ''}>
        <a href="/"><Home size={17} />Discover</a>
        {user && configuration.uploadsEnabled && <a href="/upload"><Upload size={17} />Upload</a>}
        {user && <a href="/my-uploads"><UserRound size={17} />My uploads</a>}
        {user?.isAdministrator && <a href="/admin"><Shield size={17} />Admin</a>}
      </nav>
      <div className="header-actions">
        <form action="/search" className="search"><Search size={17} /><input name="tag" aria-label="Search" placeholder="Search media or tags" /></form>
        {user ? <button className="user-button" onClick={signOut}><span>{user.name[0].toUpperCase()}</span><span className="desktop-only">{user.name}</span><LogOut size={16} /></button> : <a className="button ghost" href="/signin">Sign in</a>}
        <button className="menu" onClick={() => setOpen(!open)} aria-label="Menu">{open ? <X /> : <Menu />}</button>
      </div>
    </header>
    <main>{children}</main>
    <footer><a className="brand" href="/"><span className="brand-mark"><Headphones size={18} /></span><span>aura</span></a><p>Media worth sharing.</p><div><a href="/privacy">Privacy</a><a href="/terms">Terms of Use</a></div></footer>
  </div>;
}
