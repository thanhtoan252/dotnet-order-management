import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Boxes, LucideAngularModule, Plus, Sliders } from 'lucide-angular';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { InventoryItem } from '../../models/inventory.model';

/** Inventory list on a sortable PrimeNG p-table with a stock-level badge. */
@Component({
  selector: 'app-inventory-table',
  imports: [TableModule, TooltipModule, LucideAngularModule],
  templateUrl: './inventory-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InventoryTableComponent {
  readonly items = input.required<InventoryItem[]>();
  readonly loading = input(false);
  readonly canManage = input(false);
  readonly receive = output<InventoryItem>();
  readonly adjust = output<InventoryItem>();

  protected readonly Boxes = Boxes;
  protected readonly Plus = Plus;
  protected readonly Sliders = Sliders;

  protected badge(qty: number): string {
    if (qty <= 0) return 'bg-red-100 text-red-700';
    if (qty < 10) return 'bg-amber-100 text-amber-700';
    return 'bg-emerald-100 text-emerald-700';
  }
}
