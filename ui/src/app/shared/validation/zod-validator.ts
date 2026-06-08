import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import type { ZodType } from 'zod';

/**
 * Adapts a Zod schema into an Angular reactive-forms validator.
 * Attach a field schema to a control; on failure it emits `{ zod: <message> }`,
 * which templates surface via `control.getError('zod')`.
 */
export function zodValidator(schema: ZodType): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const result = schema.safeParse(control.value);
    return result.success ? null : { zod: result.error.issues[0]?.message ?? 'Invalid value' };
  };
}
