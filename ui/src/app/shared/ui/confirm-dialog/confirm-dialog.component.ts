import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

export interface ConfirmData {
  title: string;
  message: string;
  confirmText?: string;
  danger?: boolean;
}

/**
 * Generic confirmation dialog. Opened via DialogService; the boolean result is
 * delivered through `ref.onClose`. The title is passed as the dialog `header`
 * by the caller, so this component only renders the message + actions.
 */
@Component({
  selector: 'app-confirm-dialog',
  imports: [ButtonModule],
  templateUrl: './confirm-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialogComponent {
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly data = this.config.data as ConfirmData;

  cancel(): void {
    this.ref.close(false);
  }

  confirm(): void {
    this.ref.close(true);
  }
}
