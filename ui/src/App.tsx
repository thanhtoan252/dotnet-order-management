import { useState, useEffect } from 'react';
import { Package, ShoppingBag, Boxes, Menu, ChevronLeft, LayoutGrid, User, LogOut } from 'lucide-react';
import { useAuth } from './features/auth/useAuth';
import { ProductsManager } from './features/product-management/components/ProductsManager';
import { OrdersManager } from './features/order-management/components/OrdersManager';
import { InventoryManager } from './features/inventory-management/components/InventoryManager';
import './App.css';

type Page = 'products' | 'orders' | 'inventory';

const VALID_PAGES: Page[] = ['products', 'orders', 'inventory'];

const navItems: { id: Page; label: string; icon: React.ElementType; description: string }[] = [
  { id: 'products', label: 'Products', icon: Package, description: 'Manage product catalog' },
  { id: 'orders', label: 'Orders', icon: ShoppingBag, description: 'View and manage orders' },
  { id: 'inventory', label: 'Inventory', icon: Boxes, description: 'Track on-hand and reserved stock' },
];

function getPageFromHash(): Page {
  const hash = window.location.hash.replace('#', '') as Page;
  return VALID_PAGES.includes(hash) ? hash : 'products';
}

function App() {
  const [page, setPage] = useState<Page>(getPageFromHash);

  useEffect(() => {
    const onHashChange = () => setPage(getPageFromHash());
    window.addEventListener('hashchange', onHashChange);
    return () => window.removeEventListener('hashchange', onHashChange);
  }, []);

  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  const navigate = (id: Page) => {
    setMobileOpen(false);
    // Defer hash update so it's not a direct render-time side effect
    queueMicrotask(() => { window.location.hash = id; });
  };
  const { username, logout } = useAuth();

  const current = navItems.find(n => n.id === page)!;

  return (
    <div className="flex h-screen bg-slate-50 overflow-hidden">
      {/* Mobile overlay */}
      {mobileOpen && (
        <div
          className="fixed inset-0 bg-black/30 z-20 lg:hidden"
          onClick={() => setMobileOpen(false)}
        />
      )}

      {/* ── Sidebar ─────────────────────────────────────────────────── */}
      <aside
        className={[
          'fixed lg:relative inset-y-0 left-0 z-30',
          'flex flex-col bg-white border-r border-slate-200',
          'transition-all duration-300 ease-in-out',
          collapsed ? 'w-16' : 'w-64',
          mobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
        ].join(' ')}
      >
        {/* Brand */}
        <div className={`flex items-center gap-3 px-4 py-5 border-b border-slate-100 ${collapsed ? 'justify-center' : ''}`}>
          <div className="w-9 h-9 bg-indigo-600 rounded-xl flex items-center justify-center flex-shrink-0 shadow-sm">
            <LayoutGrid className="w-5 h-5 text-white" />
          </div>
          {!collapsed && (
            <div className="overflow-hidden">
              <p className="text-sm font-bold text-slate-900 leading-tight truncate">Order Management</p>
              <p className="text-xs text-slate-400 truncate">Enterprise System</p>
            </div>
          )}
        </div>

        {/* Nav */}
        <nav className="flex-1 py-3 px-2 space-y-0.5 overflow-y-auto">
          {!collapsed && (
            <p className="px-3 pb-2 text-xs font-semibold text-slate-400 uppercase tracking-wider">Menu</p>
          )}
          {navItems.map(({ id, label, icon: Icon }) => (
            <button
              key={id}
              onClick={() => navigate(id)}
              title={collapsed ? label : undefined}
              className={[
                'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-150',
                collapsed ? 'justify-center' : '',
                page === id
                  ? 'bg-indigo-50 text-indigo-700 shadow-sm'
                  : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
              ].join(' ')}
            >
              <Icon className={`w-5 h-5 flex-shrink-0 ${page === id ? 'text-indigo-600' : ''}`} />
              {!collapsed && <span>{label}</span>}
              {!collapsed && page === id && (
                <span className="ml-auto w-1.5 h-1.5 rounded-full bg-indigo-600" />
              )}
            </button>
          ))}
        </nav>

        {/* Collapse toggle (desktop only) */}
        <button
          className="hidden lg:flex items-center justify-center py-3 border-t border-slate-100 text-slate-400 hover:text-slate-600 hover:bg-slate-50 transition-colors"
          onClick={() => setCollapsed(v => !v)}
          title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          <ChevronLeft className={`w-4 h-4 transition-transform duration-300 ${collapsed ? 'rotate-180' : ''}`} />
        </button>
      </aside>

      {/* ── Main ────────────────────────────────────────────────────── */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        {/* Top bar */}
        <header className="bg-white border-b border-slate-200 px-4 lg:px-6 py-0 flex items-center gap-4 flex-shrink-0 h-16">
          {/* Mobile hamburger */}
          <button
            className="lg:hidden p-2 -ml-1 rounded-lg text-slate-500 hover:bg-slate-100 transition-colors"
            onClick={() => setMobileOpen(v => !v)}
          >
            <Menu className="w-5 h-5" />
          </button>

          {/* Page title */}
          <div className="flex items-center gap-2.5">
            <div className="w-8 h-8 bg-indigo-50 rounded-lg flex items-center justify-center">
              <current.icon className="w-4 h-4 text-indigo-600" />
            </div>
            <div>
              <h1 className="text-sm font-semibold text-slate-900 leading-tight">{current.label}</h1>
              <p className="text-xs text-slate-400 leading-tight hidden sm:block">{current.description}</p>
            </div>
          </div>

          {/* Breadcrumb + user */}
          <div className="ml-auto flex items-center gap-4">
            <div className="flex items-center gap-2 text-xs text-slate-400">
              <span className="hidden sm:inline">Order Management</span>
              <span className="hidden sm:inline">/</span>
              <span className="font-medium text-slate-600">{current.label}</span>
            </div>
            <div className="flex items-center gap-2 pl-4 border-l border-slate-200">
              <div className="w-7 h-7 bg-indigo-100 rounded-full flex items-center justify-center">
                <User className="w-3.5 h-3.5 text-indigo-600" />
              </div>
              <span className="text-xs font-medium text-slate-700 hidden sm:inline">{username}</span>
              <button
                onClick={logout}
                title="Logout"
                className="p-1 text-slate-400 hover:text-slate-600 transition-colors rounded"
              >
                <LogOut className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        </header>

        {/* Content */}
        <main className="flex-1 overflow-auto p-4 lg:p-6">
          {page === 'products' && <ProductsManager />}
          {page === 'orders' && <OrdersManager />}
          {page === 'inventory' && <InventoryManager />}
        </main>
      </div>
    </div>
  );
}

export default App;
