import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { api } from '../api';
import type { User } from '../types';

type AuthContextValue = { user: User | null; signIn(email: string, password: string): Promise<void>; signOut(): void };
const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => JSON.parse(localStorage.getItem('aura-user') ?? 'null') as User | null);
  const signIn = async (email: string, password: string) => {
    const response = await api.login(email, password);
    localStorage.setItem('aura-token', response.token);
    localStorage.setItem('aura-user', JSON.stringify(response.user));
    setUser(response.user);
  };
  const signOut = () => { localStorage.removeItem('aura-user'); localStorage.removeItem('aura-token'); setUser(null); };
  const value = useMemo(() => ({ user, signIn, signOut }), [user]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() { const value = useContext(AuthContext); if (!value) throw new Error('AuthProvider is required.'); return value; }
