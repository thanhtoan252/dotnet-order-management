import { request } from './request';

export const apiClient = {
  get<T>(url: string, config?: { params?: Record<string, string | number> }): Promise<{ data: T }> {
    return request<T>('GET', url, undefined, config?.params);
  },
  post<T>(url: string, body?: unknown): Promise<{ data: T }> {
    return request<T>('POST', url, body);
  },
  put<T>(url: string, body?: unknown): Promise<{ data: T }> {
    return request<T>('PUT', url, body);
  },
  delete<T>(url: string): Promise<{ data: T }> {
    return request<T>('DELETE', url);
  },
};
