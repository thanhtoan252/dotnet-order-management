import { useState, useCallback, useEffect, useRef } from 'react';
import { toast } from 'sonner';
import type { Order, CreateOrderRequest } from '../types';
import type { Product } from '../../product-management/types';
import { fetchProductsApi } from '../../product-management';
import {
  fetchOrdersApi, placeOrderApi,
  confirmOrderApi, shipOrderApi, deliverOrderApi, cancelOrderApi, deleteOrderApi,
} from '../api';

const PAGE_SIZE = 20;

export const useOrdersManager = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasMore, setHasMore] = useState(true);
  const pageRef = useRef(1);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    pageRef.current = 1;
    try {
      const data = await fetchOrdersApi(1, PAGE_SIZE);
      setOrders(data);
      setHasMore(data.length === PAGE_SIZE);
      pageRef.current = 2;
    } catch {
      setError('Failed to load orders.');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadMore = useCallback(async () => {
    if (loading || !hasMore) return;
    setLoading(true);
    try {
      const data = await fetchOrdersApi(pageRef.current, PAGE_SIZE);
      if (data.length === 0) { setHasMore(false); return; }
      setOrders(prev => [...prev, ...data]);
      setHasMore(data.length === PAGE_SIZE);
      pageRef.current += 1;
    } finally {
      setLoading(false);
    }
  }, [loading, hasMore]);

  useEffect(() => {
    refresh();
    fetchProductsApi().then(setProducts).catch(() => {});
  }, [refresh]);

  const updateOrder = async (fn: () => Promise<Order>, successMsg: string): Promise<void> => {
    setLoading(true);
    setError(null);
    try {
      const updated = await fn();
      setOrders(prev => prev.map(o => (o.id === updated.id ? updated : o)));
      toast.success(successMsg);
    } catch (e: any) {
      const msg = e?.response?.data?.detail || e?.message || 'Action failed.';
      setError(msg);
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  const placeOrder = async (form: CreateOrderRequest): Promise<string | null> => {
    setLoading(true);
    try {
      const order = await placeOrderApi(form);
      setOrders(prev => [order, ...prev]);
      toast.success('Order placed successfully');
      return null;
    } catch (e: any) {
      const msg = e?.response?.data?.detail || 'Failed to place order.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const cancelOrder = async (id: string, reason: string): Promise<void> => {
    await updateOrder(() => cancelOrderApi(id, reason || 'Cancelled by user'), 'Order cancelled');
  };

  const deleteOrder = async (id: string): Promise<string | null> => {
    setLoading(true);
    setError(null);
    try {
      await deleteOrderApi(id);
      setOrders(prev => prev.filter(o => o.id !== id));
      toast.success('Order deleted');
      return null;
    } catch (e: any) {
      const msg = e?.response?.data?.detail || 'Failed to delete order.';
      setError(msg);
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  return {
    orders, products, loading, error, hasMore,
    refresh, loadMore,
    placeOrder, cancelOrder, deleteOrder,
    confirmOrder: (id: string) => updateOrder(() => confirmOrderApi(id), 'Order confirmed'),
    shipOrder: (id: string) => updateOrder(() => shipOrderApi(id), 'Order shipped'),
    deliverOrder: (id: string) => updateOrder(() => deliverOrderApi(id), 'Order delivered'),
  };
};
