import Layout from './components/Layout';
import { ErrorState } from './components/States';
import { useAuth } from './context/AuthContext';
import { useConfiguration } from './hooks';
import AdminPage from './pages/AdminPage';
import AuthPage from './pages/AuthPage';
import HomePage from './pages/HomePage';
import LegalPage from './pages/LegalPage';
import MediaPage from './pages/MediaPage';
import MyUploadsPage from './pages/MyUploadsPage';
import PasswordResetPage from './pages/PasswordResetPage';
import SearchPage from './pages/SearchPage';
import UploadPage from './pages/UploadPage';

export default function App() {
  const { user } = useAuth(); const configuration = useConfiguration(); const path = location.pathname; let page;
  if (path === '/') page = <HomePage />;
  else if (path.startsWith('/media/')) page = <MediaPage id={path.split('/')[2]} />;
  else if (path === '/signin') page = <AuthPage />;
  else if (path === '/register') page = configuration.registrationEnabled ? <AuthPage register /> : <ErrorState title="Registration is closed" message="New account registration is currently unavailable." />;
  else if (path === '/forgot-password') page = <PasswordResetPage />;
  else if (path === '/reset-password') page = <PasswordResetPage reset />;
  else if (path === '/upload') page = !user ? <AuthPage /> : configuration.uploadsEnabled ? <UploadPage /> : <ErrorState title="Uploads are paused" message="Media uploads are currently unavailable." />;
  else if (path === '/my-uploads') page = user ? <MyUploadsPage /> : <AuthPage />;
  else if (path === '/search') page = <SearchPage />;
  else if (path === '/admin') page = user?.isAdministrator ? <AdminPage /> : <ErrorState title="Access denied" message="Administrator access is required." />;
  else if (path === '/privacy') page = <LegalPage />;
  else if (path === '/terms') page = <LegalPage terms />;
  else page = <ErrorState title="Page not found" message="The page you requested does not exist." />;
  return <Layout>{page}</Layout>;
}
