import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';

interface Props {
  count: number;
  loading: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export const ClearOrdersModal = ({ count, loading, onClose, onConfirm }: Props) => (
  <Modal title="Clear All Orders" onClose={onClose}>
    <div className="flex items-start gap-4">
      <div className="w-11 h-11 bg-red-100 rounded-full flex items-center justify-center flex-shrink-0">
        <AlertTriangle className="w-5 h-5 text-red-600" />
      </div>
      <div>
        <p className="font-semibold text-slate-900">Delete all {count} orders?</p>
        <p className="text-sm text-slate-500 mt-1.5">
          This will permanently remove <span className="font-semibold text-slate-700">all {count} orders</span> from the system. This action cannot be undone.
        </p>
      </div>
    </div>
    <div className="flex justify-end gap-2 mt-6 pt-5 border-t border-slate-100">
      <button onClick={onClose} className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">
        Cancel
      </button>
      <button onClick={onConfirm} disabled={loading} className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 disabled:opacity-50 transition-colors">
        Delete All Orders
      </button>
    </div>
  </Modal>
);
