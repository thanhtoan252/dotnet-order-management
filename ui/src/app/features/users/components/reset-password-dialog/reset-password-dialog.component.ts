import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { injectResetPassword } from '../../data/users.queries';
import { KeycloakUser } from '../../models/user.model';
import { resetPasswordSchema } from '../../validation/user.schema';

/** Ports ResetPasswordModal. */
@Component({
  selector: 'app-reset-password-dialog',
  imports: [ReactiveFormsModule, ButtonModule, CheckboxModule, InputTextModule],
  templateUrl: './reset-password-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly user = this.config.data as KeycloakUser;

  readonly mut = injectResetPassword();

  readonly form = this.fb.nonNullable.group({
    password: ['', zodValidator(resetPasswordSchema.shape.password)],
    temporary: [true],
  });

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    try {
      await this.mut.mutateAsync({
        id: this.user.id,
        dto: { password: v.password, temporary: v.temporary },
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
