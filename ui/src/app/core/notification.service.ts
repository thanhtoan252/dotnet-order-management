import { inject, Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

/**
 * Thin wrapper over PrimeNG MessageService. Feature mutations call
 * `notify.success(...)` / `notify.error(...)`; messages are rendered by the
 * single <p-toast> mounted in the root App component.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly messages = inject(MessageService);

  success(message: string): void {
    this.messages.add({ severity: 'success', summary: 'Success', detail: message, life: 4000 });
  }

  error(message: string): void {
    this.messages.add({ severity: 'error', summary: 'Error', detail: message, life: 5000 });
  }
}
