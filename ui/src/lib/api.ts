import axios from 'axios';
import keycloak from './keycloak';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
});

apiClient.interceptors.request.use(async config => {
  if (keycloak.authenticated) {
    try {
      await keycloak.updateToken(30);
    } catch {
      keycloak.login();
      return Promise.reject(new Error('Session expired'));
    }
    config.headers.Authorization = `Bearer ${keycloak.token}`;
  }
  return config;
});
