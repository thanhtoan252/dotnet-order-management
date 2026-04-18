import { useState } from 'react';
import { RefreshCw } from 'lucide-react';
import { useInventoryManager } from '../hooks/useInventoryManager';
import { usePermissions } from '../../auth/usePermissions';
import { InventoryTable } from './InventoryTable';
import { ReceiveStockModal } from './ReceiveStockModal';
import { AdjustStockModal } from './AdjustStockModal';
import type { InventoryItem } from '../types';

export const InventoryManager = () => {
  const { items, loading, refresh, receiveStock, adjustStock } = useInventoryManager();
  const { canManageInventory } = usePermissions();

  const [receiveTarget, setReceiveTarget] = useState<InventoryItem | null>(null);
  const [adjustTarget, setAdjustTarget] = useState<InventoryItem | null>(null);

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-xl font-bold text-slate-900">Inventory</h2>
          <p className="text-sm text-slate-500 mt-0.5">
            {items.length} item{items.length !== 1 ? 's' : ''} tracked
          </p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={refresh}
            disabled={loading}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 disabled:opacity-50 transition-colors shadow-sm"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </button>
        </div>
      </div>

      <div className="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <InventoryTable
          items={items}
          loading={loading}
          canManage={canManageInventory}
          onReceive={setReceiveTarget}
          onAdjust={setAdjustTarget}
        />
      </div>

      {receiveTarget && (
        <ReceiveStockModal
          item={receiveTarget}
          loading={loading}
          onClose={() => setReceiveTarget(null)}
          onSubmit={form => receiveStock(receiveTarget.productId, form)}
        />
      )}

      {adjustTarget && (
        <AdjustStockModal
          item={adjustTarget}
          loading={loading}
          onClose={() => setAdjustTarget(null)}
          onSubmit={form => adjustStock(adjustTarget.productId, form)}
        />
      )}
    </div>
  );
};
