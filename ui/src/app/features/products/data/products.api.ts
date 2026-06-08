import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateProductRequest,
  ImportProductsResponse,
  Product,
  UpdateProductRequest,
} from '../models/product.model';

/** HTTP access for the product catalog. Ports `features/product-management/api`. */
@Injectable({ providedIn: 'root' })
export class ProductsApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/products`;

  list(): Observable<Product[]> {
    return this.http.get<Product[]>(this.base);
  }

  create(req: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.base, req);
  }

  update(id: string, req: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.base}/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  import(file: File): Observable<ImportProductsResponse> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ImportProductsResponse>(`${this.base}/import`, fd);
  }
}
