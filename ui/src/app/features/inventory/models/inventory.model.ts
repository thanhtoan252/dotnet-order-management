export interface InventoryItem {
  productId: string;
  sku: string;
  productName: string;
  onHand: number;
  reserved: number;
  available: number;
}

export interface ReceiveStockRequest {
  quantity: number;
  reason?: string;
}

export interface AdjustStockRequest {
  onHand: number;
  reason: string;
}
