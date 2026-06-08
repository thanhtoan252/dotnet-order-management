import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AdjustStockRequest, InventoryItem, ReceiveStockRequest } from '../models/inventory.model';

/** HTTP access for inventory. Ports `features/inventory-management/api`. */
@Injectable({ providedIn: 'root' })
export class InventoryApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/inventory`;

  list(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>(this.base);
  }

  receive(productId: string, req: ReceiveStockRequest): Observable<InventoryItem> {
    return this.http.post<InventoryItem>(`${this.base}/${productId}/receive`, req);
  }

  adjust(productId: string, req: AdjustStockRequest): Observable<InventoryItem> {
    return this.http.post<InventoryItem>(`${this.base}/${productId}/adjust`, req);
  }
}
