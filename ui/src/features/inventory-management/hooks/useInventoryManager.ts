import { useState, useCallback, useEffect } from 'react';
import { toast } from 'sonner';
import { ApiError } from '../../../lib/api';
import type { InventoryItem, ReceiveStockRequest, AdjustStockRequest } from '../types';
import { fetchInventoryApi, receiveStockApi, adjustStockApi } from '../api';

export const useInventoryManager = () => {
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setItems(await fetchInventoryApi());
    } catch {
      setError('Failed to load inventory.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { refresh(); }, [refresh]);

  const replaceItem = (updated: InventoryItem) =>
    setItems(prev => prev.map(i => (i.productId === updated.productId ? updated : i)));

  const receiveStock = async (productId: string, req: ReceiveStockRequest): Promise<string | null> => {
    setLoading(true);
    try {
      replaceItem(await receiveStockApi(productId, req));
      toast.success('Stock received');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to receive stock.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const adjustStock = async (productId: string, req: AdjustStockRequest): Promise<string | null> => {
    setLoading(true);
    try {
      replaceItem(await adjustStockApi(productId, req));
      toast.success('Stock adjusted');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to adjust stock.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  return { items, loading, error, refresh, receiveStock, adjustStock };
};
