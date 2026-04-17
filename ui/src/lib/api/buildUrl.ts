const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

export function buildUrl(path: string, params?: Record<string, string | number>): string {
  const url = `${BASE_URL}${path}`;

  if (!params) return url;

  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    search.set(key, String(value));
  }
  return `${url}?${search.toString()}`;
}
