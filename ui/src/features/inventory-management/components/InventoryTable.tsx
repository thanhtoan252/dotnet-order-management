import { Boxes, Plus, Sliders } from 'lucide-react';
import { Tooltip } from '../../../components/Tooltip';
import type { InventoryItem } from '../types';

interface Props {
  items: InventoryItem[];
  loading: boolean;
  canManage: boolean;
  onReceive: (item: InventoryItem) => void;
  onAdjust: (item: InventoryItem) => void;
}

const stockBadgeCls = (qty: number) => {
  if (qty <= 0) return 'bg-red-100 text-red-700';
  if (qty < 10) return 'bg-amber-100 text-amber-700';
  return 'bg-emerald-100 text-emerald-700';
};

export const InventoryTable = ({ items, loading, canManage, onReceive, onAdjust }: Props) => {
  if (loading && items.length === 0) {
    return <div className="py-16 text-center text-slate-400 text-sm">Loading…</div>;
  }

  if (items.length === 0) {
    return (
      <div className="py-16 text-center">
        <Boxes className="w-10 h-10 text-slate-300 mx-auto mb-3" />
        <p className="text-slate-500 font-medium">No inventory yet</p>
        <p className="text-sm text-slate-400 mt-1">Items appear here once products are created.</p>
      </div>
    );
  }

  return (
    <>
      <table className="w-full text-sm">
        <thead>
          <tr className="bg-slate-50 border-b border-slate-200">
            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Product</th>
            <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">SKU</th>
            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">On hand</th>
            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Reserved</th>
            <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Available</th>
            {canManage && <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Actions</th>}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {items.map(i => (
            <tr key={i.productId} className="hover:bg-slate-50 transition-colors">
              <td className="px-4 py-3.5 font-medium text-slate-900">{i.productName}</td>
              <td className="px-4 py-3.5">
                <span className="font-mono text-xs bg-slate-100 text-slate-600 px-2 py-0.5 rounded">{i.sku}</span>
              </td>
              <td className="px-4 py-3.5 text-right text-slate-700">{i.onHand}</td>
              <td className="px-4 py-3.5 text-right text-slate-500">{i.reserved}</td>
              <td className="px-4 py-3.5 text-right">
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${stockBadgeCls(i.available)}`}>
                  {i.available} units
                </span>
              </td>
              {canManage && (
                <td className="px-4 py-3.5">
                  <div className="flex items-center justify-end gap-1">
                    <Tooltip label="Receive stock">
                      <button
                        onClick={() => onReceive(i)}
                        className="p-1.5 text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors cursor-pointer"
                      >
                        <Plus className="w-4 h-4" />
                      </button>
                    </Tooltip>
                    <Tooltip label="Adjust stock">
                      <button
                        onClick={() => onAdjust(i)}
                        className="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors cursor-pointer"
                      >
                        <Sliders className="w-4 h-4" />
                      </button>
                    </Tooltip>
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
      {loading && (
        <div className="border-t border-slate-100 py-3 text-center text-slate-400 text-xs">Refreshing…</div>
      )}
    </>
  );
};
