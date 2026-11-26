import { AudioLines, Compass, LogOut, Menu, Search, Shield, Upload, UserRound, X } from 'lucide-react';
import { useState, type ReactNode } from 'react';
import { useAuth } from '../context/AuthContext';
import { useConfiguration } from '../hooks';
import ApiErrorBanner from './ApiErrorBanner';

function Brand() {
  return <a className="brand" href="/" aria-label="Aura home">
    <span className="brand-mark"><AudioLines size={19} aria-hidden="true" /></span>
    <span>aura<span className="brand-period">.</span></span>
  </a>;
}

export default function Layout({ children }: { children: ReactNode }) {
  const { user, signOut } = useAuth();
  const configuration = useConfiguration();
  const [open, setOpen] = useState(false);
  const path = location.pathname;
  const links = [
    { href: '/', text: 'Discover', icon: Compass, visible: true },
    { href: '/search', text: 'Browse', icon: Search, visible: true },
    { href: '/upload', text: 'Upload', icon: Upload, visible: user && configuration.uploadsEnabled },
    { href: '/my-uploads', text: 'My uploads', icon: UserRound, visible: user },
    { href: '/admin', text: 'Admin', icon: Shield, visible: user?.isAdministrator },
  ];

  return <div className="app-shell">
    <a className="skip-link" href="#main-content">Skip to content</a>
    <ApiErrorBanner />
    <header className="site-header" onKeyDown={event => {
      if (event.key === 'Escape' && open) {
        setOpen(false);
        document.getElementById('nav-toggle')?.focus();
      }
    }}>
      <div className="header-inner">
        <Brand />
        <nav id="primary-nav" className={open ? 'open' : ''} aria-label="Primary">
          {links.filter(link => link.visible).map(({ href, text, icon: Icon }) =>
            <a href={href} key={href} aria-current={path === href ? 'page' : undefined}>
              <Icon size={16} aria-hidden="true" />{text}
            </a>
          )}
        </nav>
        <div className="header-actions">
          <form action="/search" className="search" role="search" aria-label="Site search">
            <button aria-label="Submit search"><Search size={16} /></button>
            <input type="search" name="tag" aria-label="Search media" placeholder="Search media…" />
          </form>
          {user
            ? <button className="user-button" onClick={signOut} aria-label={`Sign out ${user.name}`}>
              <span aria-hidden="true">{user.name.charAt(0).toUpperCase()}</span>
              <span className="desktop-only">{user.name}</span><LogOut size={15} aria-hidden="true" />
            </button>
            : <a className="button ghost" href="/signin">Sign in</a>}
          <button id="nav-toggle" className="menu" onClick={() => setOpen(value => !value)}
            aria-label={open ? 'Close menu' : 'Open menu'} aria-expanded={open} aria-controls="primary-nav">
            {open ? <X size={21} /> : <Menu size={21} />}
          </button>
        </div>
      </div>
    </header>
    <main id="main-content" tabIndex={-1}>{children}</main>
    <footer className="site-footer"><div className="footer-inner">
      <Brand />
      <nav aria-label="Legal"><a href="/privacy">Privacy</a><a href="/terms">Terms of Use</a></nav>
    </div></footer>
  </div>;
}
