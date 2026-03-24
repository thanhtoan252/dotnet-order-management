import { useState } from 'react';
import { Plus, RefreshCw } from 'lucide-react';
import { useOrdersManager } from '../hooks/useOrdersManager';
import { OrdersTable } from './OrdersTable';
import { CreateOrderModal } from './CreateOrderModal';
import { CancelOrderModal } from './CancelOrderModal';
import { DeleteOrderModal } from './DeleteOrderModal';
import type { Order } from '../types';

export const OrdersManager = () => {
  const {
    orders, products, loading, hasMore,
    refresh, loadMore,
    placeOrder, cancelOrder, deleteOrder,
    confirmOrder, shipOrder, deliverOrder,
  } = useOrdersManager();

  const [createOpen, setCreateOpen] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Order | null>(null);

  const handleCancel = async (reason: string) => {
    if (!cancelTarget) return;
    await cancelOrder(cancelTarget, reason);
    setCancelTarget(null);
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-xl font-bold text-slate-900">Orders</h2>
          <p className="text-sm text-slate-500 mt-0.5">{orders.length} order{orders.length !== 1 ? 's' : ''} loaded</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            onClick={refresh}
            disabled={loading}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 disabled:opacity-50 transition-colors shadow-sm"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </button>
          <button
            onClick={() => setCreateOpen(true)}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" />
            New Order
          </button>
        </div>
      </div>

<div className="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <OrdersTable
          orders={orders}
          loading={loading}
          hasMore={hasMore}
          onConfirm={confirmOrder}
          onShip={shipOrder}
          onDeliver={deliverOrder}
          onCancel={setCancelTarget}
          onDelete={setDeleteTarget}
          onLoadMore={loadMore}
        />
      </div>

      {createOpen && (
        <CreateOrderModal
          products={products}
          loading={loading}
          onClose={() => setCreateOpen(false)}
          onSubmit={placeOrder}
        />
      )}

      {cancelTarget && (
        <CancelOrderModal
          onClose={() => setCancelTarget(null)}
          onConfirm={handleCancel}
        />
      )}

      {deleteTarget && (
        <DeleteOrderModal
          order={deleteTarget}
          loading={loading}
          onClose={() => setDeleteTarget(null)}
          onConfirm={async () => {
            await deleteOrder(deleteTarget.id);
            setDeleteTarget(null);
          }}
        />
      )}

    </div>
  );
};
