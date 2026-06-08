import { z } from 'zod';

/** Validation schema for the create order form. */
export const createOrderSchema = z.object({
  shippingAddress: z.object({
    street: z.string().trim().min(1, 'Street is required'),
    city: z.string().trim().min(1, 'City is required'),
  }),
  productId: z.string().min(1, 'Select a product'),
  quantity: z.number({ message: 'Quantity must be positive' }).min(1, 'Quantity must be positive'),
});
