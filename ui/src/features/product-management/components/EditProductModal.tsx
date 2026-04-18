import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import { FormField, inputCls } from '../../../components/FormField';
import type { Product, UpdateProductRequest } from '../types';

interface Props {
  product: Product;
  loading: boolean;
  onClose: () => void;
  onSubmit: (form: UpdateProductRequest) => Promise<string | null>;
}

export const EditProductModal = ({ product, loading, onClose, onSubmit }: Props) => {
  const [form, setForm] = useState<UpdateProductRequest>({
    name: product.name,
    price: product.price,
    currency: product.currency,
  });
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (form.name !== undefined && !form.name) {
      setError('Name cannot be empty.');
      return;
    }
    if (form.price !== undefined && form.price <= 0) {
      setError('Price must be greater than 0.');
      return;
    }
    const err = await onSubmit(form);
    if (!err) {
      onClose();
    }
  };

  return (
    <Modal title="Edit Product" onClose={onClose}>
      <div className="space-y-4">
        {error && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        )}
        <div className="bg-slate-50 rounded-lg px-3 py-2 text-xs text-slate-500">
          SKU: <span className="font-mono font-semibold text-slate-700">{product.sku}</span>
        </div>
        <FormField label="Name">
          <input
            className={inputCls}
            value={form.name ?? ''}
            onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
            autoFocus
          />
        </FormField>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="Price">
            <input
              className={inputCls}
              type="number"
              min="0.01"
              step="0.01"
              value={form.price ?? ''}
              onChange={e => setForm(f => ({ ...f, price: parseFloat(e.target.value) || undefined }))}
            />
          </FormField>
          <FormField label="Currency">
            <input
              className={inputCls}
              value={form.currency ?? ''}
              onChange={e => setForm(f => ({ ...f, currency: e.target.value }))}
            />
          </FormField>
        </div>
        <p className="text-[11px] text-slate-400">
          Stock is managed on the Inventory page.
        </p>
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
          Save Changes
        </button>
      </div>
    </Modal>
  );
};
