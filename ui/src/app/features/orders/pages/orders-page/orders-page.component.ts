import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { LucideAngularModule, Plus, RefreshCw } from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { PermissionsService } from '../../../../core/auth/permissions.service';
import { ConfirmDialogComponent } from '../../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { CancelOrderDialogComponent } from '../../components/cancel-order-dialog/cancel-order-dialog.component';
import { CreateOrderDialogComponent } from '../../components/create-order-dialog/create-order-dialog.component';
import { OrdersTableComponent } from '../../components/orders-table/orders-table.component';
import {
  injectCancelOrder,
  injectConfirmOrder,
  injectDeleteOrder,
  injectDeliverOrder,
  injectOrdersInfiniteQuery,
  injectShipOrder,
} from '../../data/orders.queries';
import { Order } from '../../models/order.model';

/** Orchestrates the orders list, pagination, state-machine transitions and dialogs. Ports OrdersManager. */
@Component({
  selector: 'app-orders-page',
  imports: [ButtonModule, LucideAngularModule, OrdersTableComponent],
  templateUrl: './orders-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersPageComponent {
  protected readonly perms = inject(PermissionsService);
  private readonly dialog = inject(DialogService);

  protected readonly RefreshCw = RefreshCw;
  protected readonly Plus = Plus;

  readonly query = injectOrdersInfiniteQuery();
  readonly orders = computed(() => (this.query.data()?.pages ?? []).flat());

  protected readonly confirmMut = injectConfirmOrder();
  protected readonly shipMut = injectShipOrder();
  protected readonly deliverMut = injectDeliverOrder();
  private readonly cancelMut = injectCancelOrder();
  private readonly deleteMut = injectDeleteOrder();

  openCreate(): void {
    this.dialog.open(CreateOrderDialogComponent, {
      header: 'New Order',
      width: '560px',
      modal: true,
      dismissableMask: true,
    });
  }

  openCancel(order: Order): void {
    const ref = this.dialog.open(CancelOrderDialogComponent, {
      header: 'Cancel Order',
      width: '480px',
      modal: true,
      dismissableMask: true,
    });
    ref?.onClose.subscribe((reason: string | undefined) => {
      if (reason != null) this.cancelMut.mutate({ id: order.id, reason });
    });
  }

  confirmDelete(order: Order): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      header: 'Delete Order',
      width: '440px',
      modal: true,
      dismissableMask: true,
      data: {
        title: 'Delete Order',
        message: `Order ${order.orderNumber} will be permanently deleted. This cannot be undone.`,
        confirmText: 'Delete Order',
        danger: true,
      },
    });
    ref?.onClose.subscribe((confirmed) => {
      if (confirmed) this.deleteMut.mutate(order.id);
    });
  }
}
