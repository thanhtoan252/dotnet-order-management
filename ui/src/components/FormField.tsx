import type { ReactNode } from 'react';

export const inputCls =
  'w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg ' +
  'focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 placeholder:text-slate-400 transition-shadow';

export function FormField({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <label className="block text-xs font-semibold text-slate-600 mb-1.5 uppercase tracking-wide">{label}</label>
      {children}
    </div>
  );
}
