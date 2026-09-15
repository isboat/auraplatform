import { useEffect, useState } from 'react';
import { api } from './api';
import type { PlatformConfiguration } from './types';

const defaults: PlatformConfiguration = { id: 'platform', registrationEnabled: true, uploadsEnabled: true };
export function useConfiguration() {
  const [configuration, setConfiguration] = useState(defaults);
  useEffect(() => { api.configuration().then(setConfiguration).catch(() => undefined); }, []);
  return configuration;
}
