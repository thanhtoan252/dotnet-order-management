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
  status: 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled';
  shippingAddress: {
    street: string;
    city: string;
    province: string;
    zipCode: string;
  };
  createdAt: string;
  items: OrderItem[];
}

export interface CreateOrderRequest {
  customerId: string;
  shippingAddress: {
    street: string;
    city: string;
    province: string;
    zipCode: string;
  };
  lines: { productId: string; quantity: number; productName: string; unitPrice: number; currency: string }[];
  notes?: string;
}
