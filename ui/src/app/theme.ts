import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

/**
 * App theme preset. Aura base with the `primary` ramp remapped to Emerald.
 * The `{emerald.*}` references resolve against Aura's built-in color primitives.
 */
export const AppPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{emerald.50}',
      100: '{emerald.100}',
      200: '{emerald.200}',
      300: '{emerald.300}',
      400: '{emerald.400}',
      500: '{emerald.500}',
      600: '{emerald.600}',
      700: '{emerald.700}',
      800: '{emerald.800}',
      900: '{emerald.900}',
      950: '{emerald.950}',
    },
  },
});
