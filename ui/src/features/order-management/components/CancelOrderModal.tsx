import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';

interface Props {
  onClose: () => void;
  onConfirm: (reason: string) => void;
}

export const CancelOrderModal = ({ onClose, onConfirm }: Props) => {
  const [reason, setReason] = useState('');

  return (
    <Modal title="Cancel Order" onClose={onClose}>
      <div className="space-y-4">
        <div className="flex items-start gap-3">
          <div className="w-10 h-10 bg-amber-100 rounded-full flex items-center justify-center flex-shrink-0">
            <AlertTriangle className="w-5 h-5 text-amber-600" />
          </div>
          <p className="text-sm text-slate-600 pt-1">
            Please provide a reason for cancelling this order. This action can only be undone if the order is still pending or confirmed.
          </p>
        </div>
        <div>
          <label className="block text-xs font-semibold text-slate-600 mb-1.5 uppercase tracking-wide">Reason</label>
          <input
            className="w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 placeholder:text-slate-400 transition-shadow"
            placeholder="Reason for cancellation"
            value={reason}
            onChange={e => setReason(e.target.value)}
            autoFocus
          />
        </div>
      </div>
      <div className="flex justify-end gap-2 mt-6 pt-5 border-t border-slate-100">
        <button onClick={onClose} className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">
          Back
        </button>
        <button onClick={() => onConfirm(reason)} className="px-4 py-2 text-sm font-medium text-white bg-amber-600 rounded-lg hover:bg-amber-700 transition-colors">
          Confirm Cancel
        </button>
      </div>
    </Modal>
  );
};
