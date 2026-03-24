import { apiClient } from '../../../lib/api';
import type { Product, CreateProductRequest, UpdateProductRequest } from '../types';

export const fetchProductsApi = async (): Promise<Product[]> => {
  const { data } = await apiClient.get<Product[]>('/products');
  return data;
};

export const createProductApi = async (req: CreateProductRequest): Promise<Product> => {
  const { data } = await apiClient.post<Product>('/products', req);
  return data;
};

export const updateProductApi = async (id: string, req: UpdateProductRequest): Promise<Product> => {
  const { data } = await apiClient.put<Product>(`/products/${id}`, req);
  return data;
};

export const deleteProductApi = async (id: string): Promise<void> => {
  await apiClient.delete(`/products/${id}`);
};
