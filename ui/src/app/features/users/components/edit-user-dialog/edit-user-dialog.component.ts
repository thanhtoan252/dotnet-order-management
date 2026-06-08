import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { injectUpdateUser } from '../../data/users.queries';
import { KeycloakUser } from '../../models/user.model';
import { editUserSchema } from '../../validation/user.schema';

/** Ports EditUserModal (username is immutable). */
@Component({
  selector: 'app-edit-user-dialog',
  imports: [ReactiveFormsModule, ButtonModule, CheckboxModule, InputTextModule],
  templateUrl: './edit-user-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditUserDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly user = this.config.data as KeycloakUser;

  readonly mut = injectUpdateUser();

  readonly form = this.fb.nonNullable.group({
    firstName: [this.user.firstName ?? ''],
    lastName: [this.user.lastName ?? ''],
    email: [this.user.email ?? '', zodValidator(editUserSchema.shape.email)],
    enabled: [this.user.enabled],
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
        dto: {
          email: v.email.trim() || undefined,
          firstName: v.firstName.trim() || undefined,
          lastName: v.lastName.trim() || undefined,
          enabled: v.enabled,
        },
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
