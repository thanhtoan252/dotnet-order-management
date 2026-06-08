import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import {
  Check,
  ChevronDown,
  ChevronUp,
  LucideAngularModule,
  PackageCheck,
  ShoppingBag,
  Trash2,
  Truck,
  XCircle,
} from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { Order, OrderStatus } from '../../models/order.model';

const STATUS_STYLE: Record<OrderStatus, { badge: string; dot: string }> = {
  Pending: { badge: 'bg-amber-100 text-amber-700', dot: 'bg-amber-500' },
  Confirmed: { badge: 'bg-blue-100 text-blue-700', dot: 'bg-blue-500' },
  Shipped: { badge: 'bg-violet-100 text-violet-700', dot: 'bg-violet-500' },
  Delivered: { badge: 'bg-emerald-100 text-emerald-700', dot: 'bg-emerald-500' },
  Cancelled: { badge: 'bg-red-100 text-red-700', dot: 'bg-red-400' },
};

/** Ports OrdersTable: expandable detail rows + status-driven actions + Load More. */
@Component({
  selector: 'app-orders-table',
  imports: [DatePipe, TableModule, TooltipModule, ButtonModule, LucideAngularModule],
  templateUrl: './orders-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrdersTableComponent {
  readonly orders = input.required<Order[]>();
  readonly loading = input(false);
  readonly hasMore = input(false);
  readonly canManage = input(false);
  readonly confirm = output<string>();
  readonly ship = output<string>();
  readonly deliver = output<string>();
  readonly cancel = output<Order>();
  readonly remove = output<Order>();
  readonly loadMore = output<void>();

  protected readonly ShoppingBag = ShoppingBag;
  protected readonly ChevronDown = ChevronDown;
  protected readonly ChevronUp = ChevronUp;
  protected readonly Check = Check;
  protected readonly Truck = Truck;
  protected readonly PackageCheck = PackageCheck;
  protected readonly XCircle = XCircle;
  protected readonly Trash2 = Trash2;

  private readonly expandedId = signal<string | null>(null);

  protected isExpanded(o: Order): boolean {
    return this.expandedId() === o.id;
  }
  protected toggle(o: Order): void {
    this.expandedId.set(this.isExpanded(o) ? null : o.id);
  }
  protected style(o: Order) {
    return STATUS_STYLE[o.status] ?? STATUS_STYLE.Cancelled;
  }
  protected itemCount(o: Order): number {
    return o.items.reduce((s, i) => s + i.quantity, 0);
  }
  protected shipTo(o: Order): string {
    const a = o.shippingAddress;
    let s = `${a.street}, ${a.city}`;
    if (a.province) s += `, ${a.province}`;
    if (a.zipCode) s += ` ${a.zipCode}`;
    return s;
  }
}
