import { buildUrl } from './buildUrl';
import { handleResponse } from './handleResponse';

const REQUEST_TIMEOUT_MS = 30_000;

export async function request<T>(
  method: string,
  url: string,
  body?: unknown,
  params?: Record<string, string | number>,
): Promise<{ data: T }> {
  const headers: Record<string, string> = {};

  const token = localStorage.getItem('token');
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const isFormData = body instanceof FormData;
  if (body != null && !isFormData) {
    headers['Content-Type'] = 'application/json';
  }

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  try {
    const response = await fetch(buildUrl(url, params), {
      method,
      headers,
      body: body == null ? undefined : isFormData ? body : JSON.stringify(body),
      signal: controller.signal,
    });

    return handleResponse<T>(response);
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      throw new Error('Request timed out');
    }
    throw error;
  } finally {
    clearTimeout(timeoutId);
  }
}
