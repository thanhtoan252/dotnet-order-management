import { apiClient } from '../../../lib/api';
import type { InventoryItem, ReceiveStockRequest, AdjustStockRequest } from '../types';

export const fetchInventoryApi = async (): Promise<InventoryItem[]> => {
  const { data } = await apiClient.get<InventoryItem[]>('/inventory');
  return data;
};

export const receiveStockApi = async (productId: string, req: ReceiveStockRequest): Promise<InventoryItem> => {
  const { data } = await apiClient.post<InventoryItem>(`/inventory/${productId}/receive`, req);
  return data;
};

export const adjustStockApi = async (productId: string, req: AdjustStockRequest): Promise<InventoryItem> => {
  const { data } = await apiClient.post<InventoryItem>(`/inventory/${productId}/adjust`, req);
  return data;
};
