import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { LucideAngularModule, Plus, RefreshCw, Upload } from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { PermissionsService } from '../../../../core/auth/permissions.service';
import { ConfirmDialogComponent } from '../../../../shared/ui/confirm-dialog/confirm-dialog.component';
import { ImportProductsDialogComponent } from '../../components/import-products-dialog/import-products-dialog.component';
import {
  ProductFormData,
  ProductFormDialogComponent,
} from '../../components/product-form-dialog/product-form-dialog.component';
import { ProductsTableComponent } from '../../components/products-table/products-table.component';
import { injectDeleteProduct, injectProductsQuery } from '../../data/products.queries';
import { Product } from '../../models/product.model';

/** Orchestrates the products list + dialogs. Ports ProductsManager. */
@Component({
  selector: 'app-products-page',
  imports: [ButtonModule, LucideAngularModule, ProductsTableComponent],
  templateUrl: './products-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductsPageComponent {
  protected readonly perms = inject(PermissionsService);
  private readonly dialog = inject(DialogService);

  protected readonly RefreshCw = RefreshCw;
  protected readonly Upload = Upload;
  protected readonly Plus = Plus;

  readonly products = injectProductsQuery();
  private readonly deleteMut = injectDeleteProduct();

  count(): number {
    return this.products.data()?.length ?? 0;
  }

  openCreate(): void {
    this.dialog.open(ProductFormDialogComponent, {
      header: 'New Product',
      width: '480px',
      modal: true,
      dismissableMask: true,
      data: { mode: 'create' } satisfies ProductFormData,
    });
  }

  openEdit(product: Product): void {
    this.dialog.open(ProductFormDialogComponent, {
      header: 'Edit Product',
      width: '480px',
      modal: true,
      dismissableMask: true,
      data: { mode: 'edit', product } satisfies ProductFormData,
    });
  }

  openImport(): void {
    this.dialog.open(ImportProductsDialogComponent, {
      header: 'Import Products',
      width: '560px',
      modal: true,
      dismissableMask: true,
    });
  }

  confirmDelete(product: Product): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      header: 'Delete Product',
      width: '420px',
      modal: true,
      dismissableMask: true,
      data: {
        title: 'Delete Product',
        message: `"${product.name}" (SKU: ${product.sku}) will be permanently removed. This action cannot be undone.`,
        confirmText: 'Delete Product',
        danger: true,
      },
    });
    ref?.onClose.subscribe((confirmed) => {
      if (confirmed) this.deleteMut.mutate(product.id);
    });
  }
}
