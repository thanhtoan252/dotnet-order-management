export type OrderStatus = 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled';

export interface ShippingAddress {
  street: string;
  city: string;
  province: string;
  zipCode: string;
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  currency: string;
  lineTotal: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  customerId: string;
  totalAmount: number;
  currency: string;
  status: OrderStatus;
  shippingAddress: ShippingAddress;
  createdAt: string;
  items: OrderItem[];
}

export interface CreateOrderLine {
  productId: string;
  quantity: number;
  productName: string;
  unitPrice: number;
  currency: string;
}

export interface CreateOrderRequest {
  customerId: string;
  shippingAddress: ShippingAddress;
  lines: CreateOrderLine[];
  notes?: string;
}
