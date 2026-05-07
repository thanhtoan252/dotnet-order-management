import { useState } from 'react';
import { Plus, RefreshCw, Search } from 'lucide-react';
import { useUsersManager } from '../hooks/useUsersManager';
import { usePermissions } from '../../auth/usePermissions';
import { UsersTable } from './UsersTable';
import { CreateUserModal } from './CreateUserModal';
import { EditUserModal } from './EditUserModal';
import { ResetPasswordModal } from './ResetPasswordModal';
import { AssignRolesModal } from './AssignRolesModal';
import { DeleteUserModal } from './DeleteUserModal';
import type { KeycloakUser } from '../types';

export const UsersManager = () => {
  const {
    users,
    realmRoles,
    loading,
    search,
    setSearch,
    refresh,
    createUser,
    updateUser,
    deleteUser,
    resetPassword,
    assignRoles,
  } = useUsersManager();
  const { canManageUsers } = usePermissions();

  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<KeycloakUser | null>(null);
  const [resetTarget, setResetTarget] = useState<KeycloakUser | null>(null);
  const [rolesTarget, setRolesTarget] = useState<KeycloakUser | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<KeycloakUser | null>(null);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    const err = await deleteUser(deleteTarget.id);
    if (!err) setDeleteTarget(null);
  };

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    refresh({ search });
  };

  if (!canManageUsers) {
    return (
      <div className="bg-white border border-slate-200 rounded-xl p-10 text-center">
        <p className="text-slate-700 font-medium">Access denied</p>
        <p className="text-sm text-slate-500 mt-1">You need administrator privileges to view this page.</p>
      </div>
    );
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-xl font-bold text-slate-900">Users</h2>
          <p className="text-sm text-slate-500 mt-0.5">
            {users.length} user{users.length !== 1 ? 's' : ''} loaded
          </p>
        </div>
        <div className="flex gap-2 items-center">
          <form onSubmit={handleSearchSubmit} className="relative">
            <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="search"
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Search by username or email…"
              className="pl-8 pr-3 py-2 text-sm border border-slate-300 rounded-lg w-72 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
            />
          </form>
          <button
            onClick={() => refresh({ search })}
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
            Add User
          </button>
        </div>
      </div>

      <div className="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <UsersTable
          users={users}
          loading={loading}
          canManage={canManageUsers}
          onEdit={setEditTarget}
          onDelete={setDeleteTarget}
          onResetPassword={setResetTarget}
          onManageRoles={setRolesTarget}
        />
      </div>

      {createOpen && (
        <CreateUserModal
          loading={loading}
          realmRoles={realmRoles}
          onClose={() => setCreateOpen(false)}
          onSubmit={createUser}
        />
      )}

      {editTarget && (
        <EditUserModal
          user={editTarget}
          loading={loading}
          onClose={() => setEditTarget(null)}
          onSubmit={form => updateUser(editTarget.id, form)}
        />
      )}

      {resetTarget && (
        <ResetPasswordModal
          user={resetTarget}
          loading={loading}
          onClose={() => setResetTarget(null)}
          onSubmit={req => resetPassword(resetTarget.id, req)}
        />
      )}

      {rolesTarget && (
        <AssignRolesModal
          user={rolesTarget}
          realmRoles={realmRoles}
          loading={loading}
          onClose={() => setRolesTarget(null)}
          onSubmit={req => assignRoles(rolesTarget.id, req)}
        />
      )}

      {deleteTarget && (
        <DeleteUserModal
          user={deleteTarget}
          loading={loading}
          onClose={() => setDeleteTarget(null)}
          onConfirm={handleDelete}
        />
      )}
    </div>
  );
};
