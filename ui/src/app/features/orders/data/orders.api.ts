import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateOrderRequest, Order } from '../models/order.model';

/** HTTP access for orders. Ports `features/order-management/api`. */
@Injectable({ providedIn: 'root' })
export class OrdersApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/orders`;

  list(page = 1, pageSize = 20): Observable<Order[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<Order[]>(this.base, { params });
  }

  place(req: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.base, req);
  }

  confirm(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.base}/${id}/confirm`, {});
  }

  ship(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.base}/${id}/ship`, {});
  }

  deliver(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.base}/${id}/deliver`, {});
  }

  cancel(id: string, reason: string): Observable<Order> {
    return this.http.post<Order>(`${this.base}/${id}/cancel`, { reason });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
