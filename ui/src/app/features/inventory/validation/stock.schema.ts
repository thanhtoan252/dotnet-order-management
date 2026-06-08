import { z } from 'zod';

/** Validation schema for receiving stock. */
export const receiveStockSchema = z.object({
  quantity: z.number({ message: 'Quantity must be positive' }).min(1, 'Quantity must be positive'),
});

/** Validation schema for adjusting on-hand stock. */
export const adjustStockSchema = z.object({
  onHand: z
    .number({ message: 'Quantity cannot be negative' })
    .min(0, 'Quantity cannot be negative'),
  reason: z.string().trim().min(1, 'A reason is required for stock adjustments'),
});
