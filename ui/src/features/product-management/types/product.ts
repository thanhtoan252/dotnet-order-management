export interface Product {
  id: string;
  name: string;
  sku: string;
  price: number;
  currency: string;
  stockQuantity: number;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  price: number;
  currency: string;
  stockQuantity: number;
  description?: string;
}

export interface UpdateProductRequest {
  name?: string;
  price?: number;
  currency?: string;
  stockQuantity?: number;
}
