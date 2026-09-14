import { createContext, useContext, useMemo, useState } from 'react';
const AuthContext = createContext(null);
export function AuthProvider({children}) {
  const [user,setUser] = useState(()=>JSON.parse(localStorage.getItem('aura-user')||'null'));
  const signIn=(email)=>{const next={name:email.split('@')[0].replace(/[._-]/g,' '),email}; localStorage.setItem('aura-user',JSON.stringify(next)); setUser(next)};
  const signOut=()=>{localStorage.removeItem('aura-user');localStorage.removeItem('aura-token');setUser(null)};
  const value=useMemo(()=>({user,signIn,signOut}),[user]); return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
// Keep the context hook beside its provider so authentication has one public module.
// eslint-disable-next-line react-refresh/only-export-components
export const useAuth=()=>useContext(AuthContext);
