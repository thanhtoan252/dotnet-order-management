import { inject } from '@angular/core';
import { injectMutation, injectQuery, QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiError } from '../../../core/http/api-error';
import { NotificationService } from '../../../core/notification.service';
import { AdjustStockRequest, ReceiveStockRequest } from '../models/inventory.model';
import { InventoryApi } from './inventory.api';

export const inventoryKeys = {
  all: ['inventory'] as const,
};

export function injectInventoryQuery() {
  const api = inject(InventoryApi);
  return injectQuery(() => ({
    queryKey: inventoryKeys.all,
    queryFn: () => firstValueFrom(api.list()),
  }));
}

export function injectReceiveStock() {
  const api = inject(InventoryApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (vars: { productId: string; dto: ReceiveStockRequest }) =>
      firstValueFrom(api.receive(vars.productId, vars.dto)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: inventoryKeys.all });
      notify.success('Stock received');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectAdjustStock() {
  const api = inject(InventoryApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (vars: { productId: string; dto: AdjustStockRequest }) =>
      firstValueFrom(api.adjust(vars.productId, vars.dto)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: inventoryKeys.all });
      notify.success('Stock adjusted');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}
