import { z } from 'zod';

/** Optional email field: empty string is allowed, otherwise must look like an email. */
export const optionalEmail = z
  .string()
  .refine((v) => v === '' || /^\S+@\S+\.\S+$/.test(v), 'Email must be a valid address');
