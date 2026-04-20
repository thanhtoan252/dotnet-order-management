import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import { FormField, inputCls } from '../../../components/FormField';
import type { InventoryItem, ReceiveStockRequest } from '../types';

interface Props {
  item: InventoryItem;
  loading: boolean;
  onClose: () => void;
  onSubmit: (form: ReceiveStockRequest) => Promise<string | null>;
}

export const ReceiveStockModal = ({ item, loading, onClose, onSubmit }: Props) => {
  const [quantity, setQuantity] = useState<number>(0);
  const [reason, setReason] = useState('');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (quantity <= 0) {
      setError('Quantity must be positive.');
      return;
    }
    const err = await onSubmit({ quantity, reason: reason || undefined });
    if (!err) onClose();
  };

  return (
    <Modal title="Receive stock" onClose={onClose}>
      <div className="space-y-4">
        {error && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        )}
        <div className="bg-slate-50 rounded-lg px-3 py-2 text-xs text-slate-500">
          {item.productName} <span className="font-mono ml-2">{item.sku}</span>
          <div className="mt-1">Current on hand: <span className="font-semibold text-slate-700">{item.onHand}</span></div>
        </div>
        <FormField label="Quantity to receive *">
          <input
            className={inputCls}
            type="number"
            min="1"
            value={quantity}
            onChange={e => setQuantity(parseInt(e.target.value) || 0)}
            autoFocus
          />
        </FormField>
        <FormField label="Reason (optional)">
          <input
            className={inputCls}
            placeholder="e.g. PO-1234 receipt"
            value={reason}
            onChange={e => setReason(e.target.value)}
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
          className="px-4 py-2 text-sm font-medium text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors"
        >
          Receive
        </button>
      </div>
    </Modal>
  );
};
