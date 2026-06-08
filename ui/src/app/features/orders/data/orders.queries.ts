import { inject } from '@angular/core';
import {
  injectInfiniteQuery,
  injectMutation,
  QueryClient,
} from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiError } from '../../../core/http/api-error';
import { NotificationService } from '../../../core/notification.service';
import { CreateOrderRequest, Order } from '../models/order.model';
import { OrdersApi } from './orders.api';

export const PAGE_SIZE = 20;

export const orderKeys = {
  all: ['orders'] as const,
};

/** Page-based list with "Load More". Ports the page/loadMore logic of useOrdersManager. */
export function injectOrdersInfiniteQuery() {
  const api = inject(OrdersApi);
  return injectInfiniteQuery(() => ({
    queryKey: orderKeys.all,
    queryFn: ({ pageParam }) => firstValueFrom(api.list(pageParam, PAGE_SIZE)),
    initialPageParam: 1,
    getNextPageParam: (lastPage: Order[], allPages: Order[][]) =>
      lastPage.length === PAGE_SIZE ? allPages.length + 1 : undefined,
  }));
}

export function injectPlaceOrder() {
  const api = inject(OrdersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (dto: CreateOrderRequest) => firstValueFrom(api.place(dto)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: orderKeys.all });
      notify.success('Order placed successfully');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

/**
 * State-machine transitions (confirm → ship → deliver, plus cancel).
 * Each invalidates the list so the new status is reflected. Ports `updateOrder`.
 */
function injectTransition(
  action: (api: OrdersApi, id: string) => ReturnType<OrdersApi['confirm']>,
  successMsg: string,
) {
  const api = inject(OrdersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (id: string) => firstValueFrom(action(api, id)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: orderKeys.all });
      notify.success(successMsg);
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export const injectConfirmOrder = () =>
  injectTransition((api, id) => api.confirm(id), 'Order confirmed');
export const injectShipOrder = () => injectTransition((api, id) => api.ship(id), 'Order shipped');
export const injectDeliverOrder = () =>
  injectTransition((api, id) => api.deliver(id), 'Order delivered');

export function injectCancelOrder() {
  const api = inject(OrdersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (vars: { id: string; reason: string }) =>
      firstValueFrom(api.cancel(vars.id, vars.reason || 'Cancelled by user')),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: orderKeys.all });
      notify.success('Order cancelled');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}

export function injectDeleteOrder() {
  const api = inject(OrdersApi);
  const qc = inject(QueryClient);
  const notify = inject(NotificationService);
  return injectMutation(() => ({
    mutationFn: (id: string) => firstValueFrom(api.delete(id)),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: orderKeys.all });
      notify.success('Order deleted');
    },
    onError: (e: ApiError) => notify.error(e.detail),
  }));
}
