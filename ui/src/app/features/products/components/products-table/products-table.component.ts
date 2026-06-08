import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { LucideAngularModule, Package, Pencil, Trash2 } from 'lucide-angular';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { Product } from '../../models/product.model';

/** Products list on a sortable PrimeNG p-table, with empty/loading states. */
@Component({
  selector: 'app-products-table',
  imports: [TableModule, TooltipModule, LucideAngularModule],
  templateUrl: './products-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsTableComponent {
  readonly products = input.required<Product[]>();
  readonly loading = input(false);
  readonly canManage = input(false);
  readonly edit = output<Product>();
  readonly remove = output<Product>();

  protected readonly Package = Package;
  protected readonly Pencil = Pencil;
  protected readonly Trash2 = Trash2;
}
