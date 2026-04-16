import { Pencil, Trash2, Package } from 'lucide-react';
import { Tooltip } from '../../../components/Tooltip';
import type { Product } from '../types';

interface Props {
  products: Product[];
  loading: boolean;
  canManage: boolean;
  onEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
}

const stockBadgeCls = (qty: number) => {
  if (qty === 0) return 'bg-red-100 text-red-700';
  if (qty < 10) return 'bg-amber-100 text-amber-700';
  return 'bg-emerald-100 text-emerald-700';
};

export const ProductsTable = ({ products, loading, canManage, onEdit, onDelete }: Props) => {
  if (loading && products.length === 0) {
    return <div className="py-16 text-center text-slate-400 text-sm">Loading…</div>;
  }

  if (products.length === 0) {
    return (
      <div className="py-16 text-center">
        <Package className="w-10 h-10 text-slate-300 mx-auto mb-3" />
        <p className="text-slate-500 font-medium">No products yet</p>
        <p className="text-sm text-slate-400 mt-1">Click "Add Product" to create one.</p>
      </div>
    );
  }

  return (
    <>
      <div>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200">
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Name</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">SKU</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Price</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Stock</th>
              {canManage && <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Actions</th>}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {products.map(p => (
              <tr key={p.id} className="hover:bg-slate-50 transition-colors">
                <td className="px-4 py-3.5 font-medium text-slate-900">{p.name}</td>
                <td className="px-4 py-3.5">
                  <span className="font-mono text-xs bg-slate-100 text-slate-600 px-2 py-0.5 rounded">{p.sku}</span>
                </td>
                <td className="px-4 py-3.5 text-slate-700">
                  {p.currency} <span className="font-medium">{p.price.toFixed(2)}</span>
                </td>
                <td className="px-4 py-3.5">
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${stockBadgeCls(p.stockQuantity)}`}>
                    {p.stockQuantity} units
                  </span>
                </td>
                {canManage && (
                  <td className="px-4 py-3.5">
                    <div className="flex items-center justify-end gap-1">
                      <Tooltip label="Edit product">
                        <button
                          onClick={() => onEdit(p)}
                          className="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors cursor-pointer"
                        >
                          <Pencil className="w-4 h-4" />
                        </button>
                      </Tooltip>
                      <Tooltip label="Delete product">
                        <button
                          onClick={() => onDelete(p)}
                          className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </Tooltip>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {loading && (
        <div className="border-t border-slate-100 py-3 text-center text-slate-400 text-xs">Refreshing…</div>
      )}
    </>
  );
};
