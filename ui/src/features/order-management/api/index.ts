import { apiClient } from '../../../lib/api';
import type { Order, CreateOrderRequest } from '../types';

// ─── Orders ──────────────────────────────────────────────────────────────────

export const fetchOrdersApi = async (page: number = 1, pageSize: number = 20): Promise<Order[]> => {
  const { data } = await apiClient.get<Order[]>('/orders', { params: { page, pageSize } });
  return data;
};

export const fetchOrderApi = async (id: string): Promise<Order> => {
  const { data } = await apiClient.get<Order>(`/orders/${id}`);
  return data;
};

export const placeOrderApi = async (req: CreateOrderRequest): Promise<Order> => {
  const { data } = await apiClient.post<Order>('/orders', req);
  return data;
};

export const confirmOrderApi = async (id: string): Promise<Order> => {
  const { data } = await apiClient.post<Order>(`/orders/${id}/confirm`);
  return data;
};

export const shipOrderApi = async (id: string): Promise<Order> => {
  const { data } = await apiClient.post<Order>(`/orders/${id}/ship`);
  return data;
};

export const deliverOrderApi = async (id: string): Promise<Order> => {
  const { data } = await apiClient.post<Order>(`/orders/${id}/deliver`);
  return data;
};

export const cancelOrderApi = async (id: string, reason: string): Promise<Order> => {
  const { data } = await apiClient.post<Order>(`/orders/${id}/cancel`, { reason });
  return data;
};

export const deleteOrderApi = async (id: string): Promise<void> => {
  await apiClient.delete(`/orders/${id}`);
};
