import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { LucideAngularModule, RefreshCw } from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { PermissionsService } from '../../../../core/auth/permissions.service';
import { AdjustStockDialogComponent } from '../../components/adjust-stock-dialog/adjust-stock-dialog.component';
import { InventoryTableComponent } from '../../components/inventory-table/inventory-table.component';
import { ReceiveStockDialogComponent } from '../../components/receive-stock-dialog/receive-stock-dialog.component';
import { injectInventoryQuery } from '../../data/inventory.queries';
import { InventoryItem } from '../../models/inventory.model';

/** Orchestrates the inventory list + stock dialogs. Ports InventoryManager. */
@Component({
  selector: 'app-inventory-page',
  imports: [ButtonModule, LucideAngularModule, InventoryTableComponent],
  templateUrl: './inventory-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InventoryPageComponent {
  protected readonly perms = inject(PermissionsService);
  private readonly dialog = inject(DialogService);

  protected readonly RefreshCw = RefreshCw;

  readonly inventory = injectInventoryQuery();

  count(): number {
    return this.inventory.data()?.length ?? 0;
  }

  openReceive(item: InventoryItem): void {
    this.dialog.open(ReceiveStockDialogComponent, {
      header: 'Receive stock',
      width: '440px',
      modal: true,
      dismissableMask: true,
      data: item,
    });
  }

  openAdjust(item: InventoryItem): void {
    this.dialog.open(AdjustStockDialogComponent, {
      header: 'Adjust stock',
      width: '440px',
      modal: true,
      dismissableMask: true,
      data: item,
    });
  }
}
