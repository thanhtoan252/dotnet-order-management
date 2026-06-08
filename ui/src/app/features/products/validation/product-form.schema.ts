import { z } from 'zod';

/** Validation schema for the create/edit product form. */
export const productFormSchema = z.object({
  name: z.string().trim().min(1, 'Name is required'),
  sku: z.string().trim().min(1, 'SKU is required'),
  price: z
    .number({ message: 'Price must be greater than 0' })
    .gt(0, 'Price must be greater than 0'),
  currency: z.string().trim().min(1, 'Currency is required'),
  initialStockQuantity: z.number({ message: 'Must be 0 or more' }).min(0, 'Must be 0 or more'),
});
