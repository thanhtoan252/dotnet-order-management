import { request } from './request';

export const apiClient = {
  get<T>(url: string, config?: { params?: Record<string, string | number>; headers?: Record<string, string> }): Promise<{ data: T }> {
    return request<T>('GET', url, undefined, config?.params, config?.headers);
  },
  post<T>(url: string, body?: unknown, config?: { headers?: Record<string, string> }): Promise<{ data: T }> {
    return request<T>('POST', url, body, undefined, config?.headers);
  },
  put<T>(url: string, body?: unknown, config?: { headers?: Record<string, string> }): Promise<{ data: T }> {
    return request<T>('PUT', url, body, undefined, config?.headers);
  },
  delete<T>(url: string, config?: { headers?: Record<string, string> }): Promise<{ data: T }> {
    return request<T>('DELETE', url, undefined, undefined, config?.headers);
  },
};
