import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { injectReceiveStock } from '../../data/inventory.queries';
import { InventoryItem } from '../../models/inventory.model';
import { receiveStockSchema } from '../../validation/stock.schema';

/** Ports ReceiveStockModal. */
@Component({
  selector: 'app-receive-stock-dialog',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule],
  templateUrl: './receive-stock-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReceiveStockDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly item = this.config.data as InventoryItem;

  readonly mut = injectReceiveStock();

  readonly form = this.fb.nonNullable.group({
    quantity: [0, zodValidator(receiveStockSchema.shape.quantity)],
    reason: [''],
  });

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    try {
      await this.mut.mutateAsync({
        productId: this.item.productId,
        dto: { quantity: v.quantity, reason: v.reason || undefined },
      });
      this.ref.close(true);
    } catch {
      // toast already shown by onError
    }
  }

  cancel(): void {
    this.ref.close(false);
  }
}
