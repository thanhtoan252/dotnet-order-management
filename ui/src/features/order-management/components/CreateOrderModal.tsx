import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import { FormField, inputCls } from '../../../components/FormField';
import type { CreateOrderRequest } from '../types';
import type { Product } from '../../product-management/types';

interface Props {
  products: Product[];
  loading: boolean;
  onClose: () => void;
  onSubmit: (form: CreateOrderRequest) => Promise<string | null>;
}

const emptyForm = (): CreateOrderRequest => ({
  customerId: crypto.randomUUID(),
  shippingAddress: { street: '', city: '', province: '', zipCode: '' },
  lines: [{ productId: '', quantity: 1, productName: '', unitPrice: 0, currency: 'USD' }],
});

export const CreateOrderModal = ({ products, loading, onClose, onSubmit }: Props) => {
  const [form, setForm] = useState<CreateOrderRequest>(emptyForm);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (!form.lines[0].productId) { setError('Select a product.'); return; }
    if (!form.shippingAddress.street || !form.shippingAddress.city) {
      setError('Street and city are required.');
      return;
    }
    const err = await onSubmit(form);
    if (!err) {
      onClose();
    }
  };

  const setAddress = (field: keyof CreateOrderRequest['shippingAddress'], value: string) =>
    setForm(f => ({ ...f, shippingAddress: { ...f.shippingAddress, [field]: value } }));

  return (
    <Modal title="New Order" onClose={onClose}>
      <div className="space-y-3 max-h-[65vh] overflow-y-auto pr-1">
        {error && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        )}
        <FormField label="Customer ID">
          <input className={inputCls} value={form.customerId} onChange={e => setForm(f => ({ ...f, customerId: e.target.value }))} />
        </FormField>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="Street *">
            <input className={inputCls} placeholder="123 Main St" value={form.shippingAddress.street} onChange={e => setAddress('street', e.target.value)} />
          </FormField>
          <FormField label="City *">
            <input className={inputCls} placeholder="City" value={form.shippingAddress.city} onChange={e => setAddress('city', e.target.value)} />
          </FormField>
          <FormField label="Province">
            <input className={inputCls} placeholder="Province" value={form.shippingAddress.province} onChange={e => setAddress('province', e.target.value)} />
          </FormField>
          <FormField label="Zip Code">
            <input className={inputCls} placeholder="12345" value={form.shippingAddress.zipCode} onChange={e => setAddress('zipCode', e.target.value)} />
          </FormField>
        </div>
        <FormField label="Product *">
          <select
            className={inputCls}
            value={form.lines[0].productId}
            onChange={e => {
              const selected = products.find(p => p.id === e.target.value);
              setForm(f => ({
                ...f,
                lines: [{
                  ...f.lines[0],
                  productId: e.target.value,
                  productName: selected?.name ?? '',
                  unitPrice: selected?.price ?? 0,
                  currency: selected?.currency ?? 'USD',
                }],
              }));
            }}
          >
            <option value="">-- Select product --</option>
            {products.map(p => (
              <option key={p.id} value={p.id}>
                {p.name} — {p.currency} {p.price.toFixed(2)}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Quantity">
          <input
            className={inputCls}
            type="number"
            min="1"
            value={form.lines[0].quantity}
            onChange={e => setForm(f => ({ ...f, lines: [{ ...f.lines[0], quantity: parseInt(e.target.value) || 1 }] }))}
          />
        </FormField>
      </div>
      <div className="flex justify-end gap-2 mt-6 pt-5 border-t border-slate-100">
        <button onClick={onClose} className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">
          Cancel
        </button>
        <button onClick={handleSubmit} disabled={loading} className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors">
          Place Order
        </button>
      </div>
    </Modal>
  );
};
