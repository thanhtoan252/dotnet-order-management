import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import {
  Download,
  FileSpreadsheet,
  LucideAngularModule,
  TriangleAlert,
  Upload,
} from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { ApiError } from '../../../../core/http/api-error';
import { NotificationService } from '../../../../core/notification.service';
import { injectImportProducts } from '../../data/products.queries';

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const XLSX_MIME = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

interface RowError {
  rowNumber: number;
  field: string;
  message: string;
}

function parseRowErrors(errors: Record<string, string[]>): RowError[] {
  const result: RowError[] = [];
  for (const [key, messages] of Object.entries(errors)) {
    const match = key.match(/^row\[(\d+)\]\.(.+)$/);
    if (match) {
      result.push({ rowNumber: parseInt(match[1], 10), field: match[2], message: messages[0] });
    }
  }
  return result.sort((a, b) => a.rowNumber - b.rowNumber || a.field.localeCompare(b.field));
}

/** Ports ImportProductsModal: client-side validation + inline server row errors. */
@Component({
  selector: 'app-import-products-dialog',
  imports: [ButtonModule, LucideAngularModule],
  templateUrl: './import-products-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImportProductsDialogComponent {
  private readonly ref = inject(DynamicDialogRef);
  private readonly notify = inject(NotificationService);

  protected readonly Download = Download;
  protected readonly Upload = Upload;
  protected readonly FileSpreadsheet = FileSpreadsheet;
  protected readonly TriangleAlert = TriangleAlert;
  protected readonly accept = `.xlsx,${XLSX_MIME}`;

  readonly importMut = injectImportProducts();

  readonly file = signal<File | null>(null);
  readonly clientError = signal<string | null>(null);
  private readonly serverErrors = signal<Record<string, string[]> | null>(null);

  readonly rowErrors = computed(() => {
    const e = this.serverErrors();
    return e ? parseRowErrors(e) : [];
  });
  readonly fileErrors = computed(() => this.serverErrors()?.['file'] ?? []);

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const f = input.files?.[0] ?? null;
    this.serverErrors.set(null);
    this.clientError.set(null);

    if (!f) {
      this.file.set(null);
      return;
    }
    if (!f.name.toLowerCase().endsWith('.xlsx')) {
      this.clientError.set('Only .xlsx files are accepted.');
      this.file.set(null);
      return;
    }
    if (f.size > MAX_FILE_SIZE) {
      this.clientError.set('File exceeds the 5 MB limit.');
      this.file.set(null);
      return;
    }
    this.file.set(f);
  }

  async doImport(): Promise<void> {
    const f = this.file();
    if (!f) return;
    try {
      await this.importMut.mutateAsync(f);
      this.ref.close(true);
    } catch (e) {
      if (e instanceof ApiError && e.problems?.errors) {
        this.serverErrors.set(e.problems.errors);
      } else {
        this.notify.error(e instanceof ApiError ? e.detail : 'Import failed.');
      }
    }
  }

  cancel(): void {
    this.ref.close();
  }
}
