import React, { useState } from 'react';
import { ChevronDown, ChevronUp, ShoppingBag, Check, Truck, PackageCheck, XCircle, Trash2 } from 'lucide-react';
import { Tooltip } from '../../../components/Tooltip';
import type { Order } from '../types';

interface Props {
  orders: Order[];
  loading: boolean;
  hasMore: boolean;
  onConfirm: (id: string) => void;
  onShip: (id: string) => void;
  onDeliver: (id: string) => void;
  onCancel: (id: string) => void;
  onDelete: (order: Order) => void;
  onLoadMore: () => void;
}

const STATUS_STYLE: Record<string, { bg: string; text: string; dot: string }> = {
  Pending:   { bg: 'bg-amber-100',   text: 'text-amber-700',   dot: 'bg-amber-500' },
  Confirmed: { bg: 'bg-blue-100',    text: 'text-blue-700',    dot: 'bg-blue-500' },
  Shipped:   { bg: 'bg-violet-100',  text: 'text-violet-700',  dot: 'bg-violet-500' },
  Delivered: { bg: 'bg-emerald-100', text: 'text-emerald-700', dot: 'bg-emerald-500' },
  Cancelled: { bg: 'bg-red-100',     text: 'text-red-700',     dot: 'bg-red-400' },
};

export const OrdersTable = ({ orders, loading, hasMore, onConfirm, onShip, onDeliver, onCancel, onDelete, onLoadMore }: Props) => {
  const [expandedId, setExpandedId] = useState<string | null>(null);

  if (loading && orders.length === 0) {
    return <div className="py-16 text-center text-slate-400 text-sm">Loading…</div>;
  }

  if (orders.length === 0) {
    return (
      <div className="py-16 text-center">
        <ShoppingBag className="w-10 h-10 text-slate-300 mx-auto mb-3" />
        <p className="text-slate-500 font-medium">No orders yet</p>
        <p className="text-sm text-slate-400 mt-1">Click "New Order" to place one.</p>
      </div>
    );
  }

  return (
    <>
      <div>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200">
              <th className="w-10 px-3 py-3" />
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Order #</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Date</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Status</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Items</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Total</th>
              <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {orders.map(order => {
              const st = STATUS_STYLE[order.status] ?? STATUS_STYLE['Cancelled'];
              const isExpanded = expandedId === order.id;
              return (
                <React.Fragment key={order.id}>
                  <tr className="hover:bg-slate-50 transition-colors">
                    <td className="px-3 py-3">
                      <button
                        onClick={() => setExpandedId(isExpanded ? null : order.id)}
                        className="p-1 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded transition-colors"
                      >
                        {isExpanded ? <ChevronUp className="w-3.5 h-3.5" /> : <ChevronDown className="w-3.5 h-3.5" />}
                      </button>
                    </td>
                    <td className="px-4 py-3">
                      <span className="font-mono text-xs bg-slate-100 text-slate-700 px-2 py-0.5 rounded font-semibold">
                        {order.orderNumber}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-slate-500 text-xs whitespace-nowrap">
                      {new Date(order.createdAt).toLocaleString()}
                    </td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${st.bg} ${st.text}`}>
                        <span className={`w-1.5 h-1.5 rounded-full ${st.dot}`} />
                        {order.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-slate-700">
                      {order.items.reduce((s, i) => s + i.quantity, 0)}
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-900">
                      {order.currency} {order.totalAmount.toFixed(2)}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-end gap-2">
                        {order.status === 'Pending' && (
                          <button onClick={() => onConfirm(order.id)} className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-700 hover:bg-blue-200 transition-colors cursor-pointer">
                            <Check className="w-3 h-3" /> Confirm
                          </button>
                        )}
                        {order.status === 'Confirmed' && (
                          <button onClick={() => onShip(order.id)} className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-violet-100 text-violet-700 hover:bg-violet-200 transition-colors cursor-pointer">
                            <Truck className="w-3 h-3" /> Ship
                          </button>
                        )}
                        {order.status === 'Shipped' && (
                          <button onClick={() => onDeliver(order.id)} className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-700 hover:bg-emerald-200 transition-colors cursor-pointer">
                            <PackageCheck className="w-3 h-3" /> Deliver
                          </button>
                        )}
                        {(order.status === 'Pending' || order.status === 'Confirmed') && (
                          <Tooltip label="Cancel order">
                            <button onClick={() => onCancel(order.id)} className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer">
                              <XCircle className="w-4 h-4" />
                            </button>
                          </Tooltip>
                        )}
                        <Tooltip label="Delete order">
                          <button onClick={() => onDelete(order)} className="p-1.5 text-slate-400 hover:text-red-700 hover:bg-red-50 rounded-lg transition-colors cursor-pointer">
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </Tooltip>
                      </div>
                    </td>
                  </tr>

                  {isExpanded && (
                    <tr>
                      <td colSpan={7} className="p-0">
                        <div className="bg-slate-50 border-t border-b border-slate-100 px-6 py-4 space-y-3">
                          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                            <div>
                              <span className="font-semibold text-slate-500 uppercase tracking-wide">Customer</span>
                              <p className="font-mono text-slate-700 mt-0.5 break-all">{order.customerId}</p>
                            </div>
                            <div>
                              <span className="font-semibold text-slate-500 uppercase tracking-wide">Ship to</span>
                              <p className="text-slate-700 mt-0.5">
                                {order.shippingAddress.street}, {order.shippingAddress.city}
                                {order.shippingAddress.province ? `, ${order.shippingAddress.province}` : ''}
                                {order.shippingAddress.zipCode ? ` ${order.shippingAddress.zipCode}` : ''}
                              </p>
                            </div>
                          </div>
                          <table className="w-full text-xs border border-slate-200 rounded-lg overflow-hidden">
                            <thead>
                              <tr className="bg-slate-100">
                                <th className="text-left px-3 py-2 font-semibold text-slate-500 uppercase tracking-wide">Product</th>
                                <th className="text-left px-3 py-2 font-semibold text-slate-500 uppercase tracking-wide">Qty</th>
                                <th className="text-left px-3 py-2 font-semibold text-slate-500 uppercase tracking-wide">Unit Price</th>
                                <th className="text-left px-3 py-2 font-semibold text-slate-500 uppercase tracking-wide">Line Total</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-100">
                              {order.items.map((item, i) => (
                                <tr key={i} className="bg-white">
                                  <td className="px-3 py-2 text-slate-700 font-medium">{item.productName}</td>
                                  <td className="px-3 py-2 text-slate-600">{item.quantity}</td>
                                  <td className="px-3 py-2 text-slate-600">{item.currency} {item.unitPrice.toFixed(2)}</td>
                                  <td className="px-3 py-2 font-semibold text-slate-800">{item.currency} {item.lineTotal.toFixed(2)}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      </td>
                    </tr>
                  )}
                </React.Fragment>
              );
            })}
          </tbody>
        </table>
      </div>

      {loading && (
        <div className="border-t border-slate-100 py-3 text-center text-slate-400 text-xs">Loading…</div>
      )}
      {!loading && hasMore && (
        <div className="border-t border-slate-100 py-3 text-center">
          <button onClick={onLoadMore} className="px-4 py-2 text-sm font-medium text-slate-600 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors">
            Load More
          </button>
        </div>
      )}
      {!loading && !hasMore && (
        <p className="text-center text-xs text-slate-400 py-3 border-t border-slate-100">
          All {orders.length} orders loaded
        </p>
      )}
    </>
  );
};
