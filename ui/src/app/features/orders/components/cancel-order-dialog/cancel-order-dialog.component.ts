import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { LucideAngularModule, TriangleAlert } from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';

/** Ports CancelOrderModal: collects a cancellation reason and returns it via onClose. */
@Component({
  selector: 'app-cancel-order-dialog',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, LucideAngularModule],
  templateUrl: './cancel-order-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CancelOrderDialogComponent {
  private readonly ref = inject(DynamicDialogRef);
  protected readonly TriangleAlert = TriangleAlert;
  readonly reason = new FormControl('', { nonNullable: true });

  cancel(): void {
    this.ref.close();
  }

  confirm(): void {
    this.ref.close(this.reason.value);
  }
}
