import { useState } from 'react';
import { Plus, RefreshCw } from 'lucide-react';
import { useProductsManager } from '../hooks/useProductsManager';
import { ProductsTable } from './ProductsTable';
import { CreateProductModal } from './CreateProductModal';
import { EditProductModal } from './EditProductModal';
import { DeleteProductModal } from './DeleteProductModal';
import type { Product } from '../types';

export const ProductsManager = () => {
  const { products, loading, refresh, createProduct, updateProduct, deleteProduct } = useProductsManager();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<Product | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    const err = await deleteProduct(deleteTarget.id);
    if (!err) setDeleteTarget(null);
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-xl font-bold text-slate-900">Products</h2>
          <p className="text-sm text-slate-500 mt-0.5">
            {products.length} product{products.length !== 1 ? 's' : ''} total
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
          <button
            onClick={() => setCreateOpen(true)}
            className="inline-flex items-center gap-2 px-3 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
          >
            <Plus className="w-4 h-4" />
            Add Product
          </button>
        </div>
      </div>

<div className="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <ProductsTable
          products={products}
          loading={loading}
          onEdit={setEditTarget}
          onDelete={setDeleteTarget}
        />
      </div>

      {createOpen && (
        <CreateProductModal
          loading={loading}
          onClose={() => setCreateOpen(false)}
          onSubmit={createProduct}
        />
      )}

      {editTarget && (
        <EditProductModal
          product={editTarget}
          loading={loading}
          onClose={() => setEditTarget(null)}
          onSubmit={form => updateProduct(editTarget.id, form)}
        />
      )}

      {deleteTarget && (
        <DeleteProductModal
          product={deleteTarget}
          loading={loading}
          onClose={() => setDeleteTarget(null)}
          onConfirm={handleDelete}
        />
      )}
    </div>
  );
};
