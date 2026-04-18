export interface Product {
  id: string;
  name: string;
  sku: string;
  price: number;
  currency: string;
  description?: string;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  price: number;
  currency: string;
  initialStockQuantity?: number;
  description?: string;
}

export interface UpdateProductRequest {
  name?: string;
  price?: number;
  currency?: string;
}
