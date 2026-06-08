import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { injectProductsQuery } from '../../../products/data/products.queries';
import { injectPlaceOrder } from '../../data/orders.queries';
import { createOrderSchema } from '../../validation/order-form.schema';

/** Ports CreateOrderModal — single-line order against the product catalog. */
@Component({
  selector: 'app-create-order-dialog',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, SelectModule],
  templateUrl: './create-order-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateOrderDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(DynamicDialogRef);

  readonly products = injectProductsQuery();
  readonly mut = injectPlaceOrder();

  readonly form = this.fb.nonNullable.group({
    customerId: [crypto.randomUUID()],
    shippingAddress: this.fb.nonNullable.group({
      street: ['', zodValidator(createOrderSchema.shape.shippingAddress.shape.street)],
      city: ['', zodValidator(createOrderSchema.shape.shippingAddress.shape.city)],
      province: [''],
      zipCode: [''],
    }),
    productId: ['', zodValidator(createOrderSchema.shape.productId)],
    quantity: [1, zodValidator(createOrderSchema.shape.quantity)],
  });

  get address() {
    return this.form.controls.shippingAddress;
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const product = (this.products.data() ?? []).find((p) => p.id === v.productId);
    try {
      await this.mut.mutateAsync({
        customerId: v.customerId,
        shippingAddress: v.shippingAddress,
        lines: [
          {
            productId: v.productId,
            quantity: v.quantity,
            productName: product?.name ?? '',
            unitPrice: product?.price ?? 0,
            currency: product?.currency ?? 'USD',
          },
        ],
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
