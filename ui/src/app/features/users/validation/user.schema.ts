import { z } from 'zod';
import { optionalEmail } from '../../../shared/validation/email';

/** Validation schema for the create user form. */
export const createUserSchema = z.object({
  username: z.string().trim().min(1, 'Username is required'),
  email: optionalEmail,
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

/** Validation schema for the edit user form. */
export const editUserSchema = z.object({
  email: optionalEmail,
});

/** Validation schema for the reset password form. */
export const resetPasswordSchema = z.object({
  password: z.string().min(6, 'Password must be at least 6 characters'),
});
