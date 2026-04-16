import { useState, useCallback, useEffect } from 'react';
import { toast } from 'sonner';
import { ApiError } from '../../../lib/api';
import type { Product, CreateProductRequest, UpdateProductRequest } from '../types';
import { fetchProductsApi, createProductApi, updateProductApi, deleteProductApi } from '../api';

export const useProductsManager = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setProducts(await fetchProductsApi());
    } catch {
      setError('Failed to load products.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { refresh(); }, [refresh]);

  const createProduct = async (form: CreateProductRequest): Promise<string | null> => {
    setLoading(true);
    try {
      const p = await createProductApi(form);
      setProducts(prev => [...prev, p]);
      toast.success('Product created');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to create product.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const updateProduct = async (id: string, form: UpdateProductRequest): Promise<string | null> => {
    setLoading(true);
    try {
      const updated = await updateProductApi(id, form);
      setProducts(prev => prev.map(p => (p.id === updated.id ? updated : p)));
      toast.success('Product updated');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to update product.';
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  const deleteProduct = async (id: string): Promise<string | null> => {
    setLoading(true);
    setError(null);
    try {
      await deleteProductApi(id);
      setProducts(prev => prev.filter(p => p.id !== id));
      toast.success('Product deleted');
      return null;
    } catch (e) {
      const msg = e instanceof ApiError ? e.detail : 'Failed to delete product.';
      setError(msg);
      toast.error(msg);
      return msg;
    } finally {
      setLoading(false);
    }
  };

  return { products, loading, error, refresh, createProduct, updateProduct, deleteProduct };
};
