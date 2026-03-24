import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import type { CreateProductRequest } from '../types';

interface Props {
  loading: boolean;
  onClose: () => void;
  onSubmit: (form: CreateProductRequest) => Promise<string | null>;
}

const inputCls =
  'w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg ' +
  'focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 placeholder:text-slate-400 transition-shadow';

const FormField = ({ label, children }: { label: string; children: React.ReactNode }) => (
  <div>
    <label className="block text-xs font-semibold text-slate-600 mb-1.5 uppercase tracking-wide">{label}</label>
    {children}
  </div>
);

const emptyForm: CreateProductRequest = { name: '', sku: '', price: 0, currency: 'USD', stockQuantity: 0 };

export const CreateProductModal = ({ loading, onClose, onSubmit }: Props) => {
  const [form, setForm] = useState<CreateProductRequest>(emptyForm);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (!form.name || !form.sku || form.price <= 0) {
      setError('Name, SKU, and a positive price are required.');
      return;
    }
    const err = await onSubmit(form);
    if (err) {
      setError(err);
    } else {
      onClose();
    }
  };

  return (
    <Modal title="New Product" onClose={onClose}>
      <div className="space-y-4">
        {error && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        )}
        <FormField label="Name *">
          <input
            className={inputCls}
            placeholder="e.g. Widget Pro"
            value={form.name}
            onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
            autoFocus
          />
        </FormField>
        <FormField label="SKU *">
          <input
            className={inputCls}
            placeholder="e.g. PROD-001"
            value={form.sku}
            onChange={e => setForm(f => ({ ...f, sku: e.target.value }))}
          />
        </FormField>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="Price *">
            <input
              className={inputCls}
              type="number"
              min="0.01"
              step="0.01"
              value={form.price}
              onChange={e => setForm(f => ({ ...f, price: parseFloat(e.target.value) || 0 }))}
            />
          </FormField>
          <FormField label="Currency">
            <input
              className={inputCls}
              value={form.currency}
              onChange={e => setForm(f => ({ ...f, currency: e.target.value }))}
            />
          </FormField>
        </div>
        <FormField label="Stock Quantity">
          <input
            className={inputCls}
            type="number"
            min="0"
            value={form.stockQuantity}
            onChange={e => setForm(f => ({ ...f, stockQuantity: parseInt(e.target.value) || 0 }))}
          />
        </FormField>
      </div>
      <div className="flex justify-end gap-2 mt-6 pt-5 border-t border-slate-100">
        <button
          onClick={onClose}
          className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
        >
          Cancel
        </button>
        <button
          onClick={handleSubmit}
          disabled={loading}
          className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          Create Product
        </button>
      </div>
    </Modal>
  );
};
