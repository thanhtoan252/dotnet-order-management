import { inject } from '@angular/core';
import { injectMutation, injectQuery, QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiError } from '../../../core/http/api-error';
import { NotificationService } from '../../../core/notification.service';
import {
  CreateProductRequest,
  ImportProductsResponse,
  UpdateProductRequest,
} from '../models/product.model';
import { ProductsApi } from './products.api';

export const productKeys = {
  all: ['products'] as const,
};

/** Replaces the fetch+useState part of `useProductsManager`: caching/dedupe are free. */
export function injectProductsQuery() {
  const api = inject(ProductsApi);
  return injectQuery(() => ({
    queryKey: productKeys.all,
    queryFn: () => firstValueFrom(api.list()),
  }));
}

export function injectCreateProduct() {
  const api = inject(ProductsApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (dto: CreateProductRequest) => firstValueFrom(api.create(dto)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.all });
      notify.success('Product created');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectUpdateProduct() {
  const api = inject(ProductsApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (vars: { id: string; dto: UpdateProductRequest }) =>
      firstValueFrom(api.update(vars.id, vars.dto)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.all });
      notify.success('Product updated');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectDeleteProduct() {
  const api = inject(ProductsApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (id: string) => firstValueFrom(api.delete(id)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.all });
      notify.success('Product deleted');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

/**
 * Import does NOT toast on failure — row-level validation errors (ProblemDetails.errors)
 * are surfaced inline by the dialog. Only success toasts. Mirrors `importProducts`.
 */
export function injectImportProducts() {
  const api = inject(ProductsApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (file: File) => firstValueFrom(api.import(file)),
    onSuccess: (r: ImportProductsResponse) => {
      qc.invalidateQueries({ queryKey: productKeys.all });
      notify.success(`Imported ${r.importedCount} product${r.importedCount !== 1 ? 's' : ''}`);
    },
  }));
}
