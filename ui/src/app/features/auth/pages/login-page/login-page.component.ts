import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, LayoutGrid } from 'lucide-angular';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { AuthService } from '../../../../core/auth/auth.service';
import { zodValidator } from '../../../../shared/validation/zod-validator';
import { loginSchema } from './login.schema';

/** Ports the React `LoginForm` to Reactive Forms + PrimeNG. */
@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    LucideAngularModule,
  ],
  templateUrl: './login-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly LayoutGrid = LayoutGrid;

  readonly form = this.fb.nonNullable.group({
    username: ['', zodValidator(loginSchema.shape.username)],
    password: ['', zodValidator(loginSchema.shape.password)],
  });
  readonly loading = signal(false);
  readonly error = signal('');

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { username, password } = this.form.getRawValue();
    this.auth.login(username, password).subscribe({
      next: () => void this.router.navigate(['/products']),
      error: () => {
        this.error.set('Invalid username or password');
        this.loading.set(false);
      },
    });
  }
}
