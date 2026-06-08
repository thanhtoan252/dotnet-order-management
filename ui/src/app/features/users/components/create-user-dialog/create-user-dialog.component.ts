import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { injectCreateUser } from '../../data/users.queries';
import { RealmRole } from '../../models/user.model';
import { createUserSchema } from '../../validation/user.schema';

export interface CreateUserData {
  realmRoles: RealmRole[];
}

/** Ports CreateUserModal. */
@Component({
  selector: 'app-create-user-dialog',
  imports: [ReactiveFormsModule, ButtonModule, CheckboxModule, InputTextModule, MultiSelectModule],
  templateUrl: './create-user-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateUserDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(DynamicDialogRef);
  private readonly config = inject(DynamicDialogConfig);
  readonly data = this.config.data as CreateUserData;

  readonly mut = injectCreateUser();

  readonly form = this.fb.nonNullable.group({
    username: ['', zodValidator(createUserSchema.shape.username)],
    firstName: [''],
    lastName: [''],
    email: ['', zodValidator(createUserSchema.shape.email)],
    password: ['', zodValidator(createUserSchema.shape.password)],
    temporaryPassword: [true],
    enabled: [true],
    roles: [[] as string[]],
  });

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    try {
      await this.mut.mutateAsync({
        username: v.username.trim(),
        email: v.email.trim() || undefined,
        firstName: v.firstName.trim() || undefined,
        lastName: v.lastName.trim() || undefined,
        password: v.password,
        temporaryPassword: v.temporaryPassword,
        enabled: v.enabled,
        roles: v.roles,
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
