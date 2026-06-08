import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { injectCreateProduct, injectUpdateProduct } from '../../data/products.queries';
import { Product } from '../../models/product.model';
import { productFormSchema } from '../../validation/product-form.schema';

export interface ProductFormData {
  mode: 'create' | 'edit';
  product?: Product;
}

/** Combined create/edit dialog. Ports CreateProductModal + EditProductModal. */
@Component({
  selector: 'app-product-form-dialog',
  imports: [ReactiveFormsModule, InputTextModule, ButtonModule],
  templateUrl: './product-form-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly data = this.config.data as ProductFormData;

  private readonly createMut = injectCreateProduct();
  private readonly updateMut = injectUpdateProduct();

  readonly isEdit = computed(() => this.data.mode === 'edit');
  readonly pending = computed(() => this.createMut.isPending() || this.updateMut.isPending());

  readonly form = this.fb.nonNullable.group({
    name: ['', zodValidator(productFormSchema.shape.name)],
    sku: ['', zodValidator(productFormSchema.shape.sku)],
    price: [0, zodValidator(productFormSchema.shape.price)],
    currency: ['USD', zodValidator(productFormSchema.shape.currency)],
    initialStockQuantity: [0, zodValidator(productFormSchema.shape.initialStockQuantity)],
  });

  constructor() {
    const p = this.data.product;
    if (this.data.mode === 'edit' && p) {
      this.form.patchValue({ name: p.name, price: p.price, currency: p.currency });
      // SKU and initial stock are not editable on an existing product.
      this.form.controls.sku.disable();
      this.form.controls.initialStockQuantity.disable();
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    try {
      if (this.isEdit() && this.data.product) {
        await this.updateMut.mutateAsync({
          id: this.data.product.id,
          dto: { name: v.name, price: v.price, currency: v.currency },
        });
      } else {
        await this.createMut.mutateAsync({
          name: v.name,
          sku: v.sku,
          price: v.price,
          currency: v.currency,
          initialStockQuantity: v.initialStockQuantity,
        });
      }
      this.ref.close(true);
    } catch {
      // onError already surfaced a toast; keep the dialog open for correction.
    }
  }

  cancel(): void {
    this.ref.close();
  }
}
