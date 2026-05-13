import { Pencil, Trash2, KeyRound, ShieldCheck, Users as UsersIcon, CheckCircle2, XCircle } from 'lucide-react';
import { Tooltip } from '../../../components/Tooltip';
import type { KeycloakUser } from '../types';

interface Props {
  users: KeycloakUser[];
  loading: boolean;
  canManage: boolean;
  onEdit: (user: KeycloakUser) => void;
  onDelete: (user: KeycloakUser) => void;
  onResetPassword: (user: KeycloakUser) => void;
  onManageRoles: (user: KeycloakUser) => void;
}

export const UsersTable = ({
  users,
  loading,
  canManage,
  onEdit,
  onDelete,
  onResetPassword,
  onManageRoles,
}: Props) => {
  if (loading && users.length === 0) {
    return <div className="py-16 text-center text-slate-400 text-sm">Loading…</div>;
  }

  if (users.length === 0) {
    return (
      <div className="py-16 text-center">
        <UsersIcon className="w-10 h-10 text-slate-300 mx-auto mb-3" />
        <p className="text-slate-500 font-medium">No users found</p>
        <p className="text-sm text-slate-400 mt-1">Try adjusting your search or add a new user.</p>
      </div>
    );
  }

  return (
    <>
      <div>
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200">
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Username</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Name</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Email</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Status</th>
              <th className="text-left px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Roles</th>
              {canManage && <th className="text-right px-4 py-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">Actions</th>}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {users.map(u => (
              <tr key={u.id} className="hover:bg-slate-50 transition-colors">
                <td className="px-4 py-3.5 font-medium text-slate-900">{u.username}</td>
                <td className="px-4 py-3.5 text-slate-700">
                  {[u.firstName, u.lastName].filter(Boolean).join(' ') || <span className="text-slate-400">—</span>}
                </td>
                <td className="px-4 py-3.5 text-slate-700">{u.email ?? <span className="text-slate-400">—</span>}</td>
                <td className="px-4 py-3.5">
                  {u.enabled ? (
                    <span className="inline-flex items-center gap-1 text-xs font-medium text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded">
                      <CheckCircle2 className="w-3 h-3" /> Enabled
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 text-xs font-medium text-slate-600 bg-slate-100 px-2 py-0.5 rounded">
                      <XCircle className="w-3 h-3" /> Disabled
                    </span>
                  )}
                </td>
                <td className="px-4 py-3.5">
                  <div className="flex flex-wrap gap-1">
                    {u.roles.length === 0
                      ? <span className="text-slate-400 text-xs">none</span>
                      : u.roles.map(r => (
                        <span
                          key={r}
                          className={[
                            'text-[11px] px-2 py-0.5 rounded font-medium',
                            r === 'admin' ? 'bg-indigo-50 text-indigo-700' : 'bg-slate-100 text-slate-600',
                          ].join(' ')}
                        >
                          {r}
                        </span>
                      ))}
                  </div>
                </td>
                {canManage && (
                  <td className="px-4 py-3.5">
                    <div className="flex items-center justify-end gap-1">
                      <Tooltip label="Edit profile">
                        <button
                          onClick={() => onEdit(u)}
                          className="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors cursor-pointer"
                        >
                          <Pencil className="w-4 h-4" />
                        </button>
                      </Tooltip>
                      <Tooltip label="Manage roles">
                        <button
                          onClick={() => onManageRoles(u)}
                          className="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors cursor-pointer"
                        >
                          <ShieldCheck className="w-4 h-4" />
                        </button>
                      </Tooltip>
                      <Tooltip label="Reset password">
                        <button
                          onClick={() => onResetPassword(u)}
                          className="p-1.5 text-slate-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors cursor-pointer"
                        >
                          <KeyRound className="w-4 h-4" />
                        </button>
                      </Tooltip>
                      <Tooltip label="Delete user">
                        <button
                          onClick={() => onDelete(u)}
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
