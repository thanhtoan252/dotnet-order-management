import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { injectAdjustStock } from '../../data/inventory.queries';
import { InventoryItem } from '../../models/inventory.model';
import { adjustStockSchema } from '../../validation/stock.schema';

/** Ports AdjustStockModal (shows the delta vs current on-hand). */
@Component({
  selector: 'app-adjust-stock-dialog',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule],
  templateUrl: './adjust-stock-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdjustStockDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly item = this.config.data as InventoryItem;

  readonly mut = injectAdjustStock();

  readonly form = this.fb.nonNullable.group({
    onHand: [this.item.onHand, zodValidator(adjustStockSchema.shape.onHand)],
    reason: ['', zodValidator(adjustStockSchema.shape.reason)],
  });

  private readonly onHandValue = toSignal(this.form.controls.onHand.valueChanges, {
    initialValue: this.item.onHand,
  });
  readonly delta = computed(() => (this.onHandValue() || 0) - this.item.onHand);

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    try {
      await this.mut.mutateAsync({
        productId: this.item.productId,
        dto: { onHand: v.onHand, reason: v.reason.trim() },
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
