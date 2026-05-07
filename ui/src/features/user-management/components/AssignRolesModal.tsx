import { useEffect, useMemo, useState } from 'react';
import { toast } from 'sonner';
import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight, Loader2, Search } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import { fetchUserRolesApi } from '../api';
import type { KeycloakUser, RealmRole, AssignRolesRequest } from '../types';

interface Props {
  user: KeycloakUser;
  realmRoles: RealmRole[];
  loading: boolean;
  onClose: () => void;
  onSubmit: (req: AssignRolesRequest) => Promise<string | null>;
}

type RoleListProps = {
  title: string;
  roles: RealmRole[];
  selected: Set<string>;
  onToggle: (name: string) => void;
  onMove: (name: string) => void;
  search: string;
  onSearchChange: (v: string) => void;
  emptyText: string;
};

const RoleList = ({ title, roles, selected, onToggle, onMove, search, onSearchChange, emptyText }: RoleListProps) => (
  <div className="flex flex-col flex-1 min-w-0 border border-slate-200 rounded-xl overflow-hidden bg-white">
    <div className="px-3 py-2 border-b border-slate-100 bg-slate-50">
      <div className="text-xs font-semibold text-slate-700 uppercase tracking-wide">
        {title} <span className="text-slate-400 font-normal normal-case">({roles.length})</span>
      </div>
    </div>
    <div className="px-2 py-2 border-b border-slate-100">
      <div className="relative">
        <Search className="absolute left-2 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-slate-400" />
        <input
          type="text"
          value={search}
          onChange={e => onSearchChange(e.target.value)}
          placeholder="Search roles…"
          className="w-full pl-7 pr-2 py-1.5 text-xs rounded-md border border-slate-200 focus:border-indigo-400 focus:ring-1 focus:ring-indigo-200 outline-none"
        />
      </div>
    </div>
    <div className="flex-1 overflow-y-auto h-72">
      {roles.length === 0 ? (
        <p className="text-xs text-slate-400 text-center py-8">{emptyText}</p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {roles.map(r => {
            const isSelected = selected.has(r.name);
            return (
              <li
                key={r.id}
                onClick={() => onToggle(r.name)}
                onDoubleClick={() => onMove(r.name)}
                className={`px-3 py-2 cursor-pointer transition-colors ${
                  isSelected ? 'bg-indigo-50 hover:bg-indigo-100' : 'hover:bg-slate-50'
                }`}
              >
                <div className={`text-sm font-medium ${isSelected ? 'text-indigo-900' : 'text-slate-900'}`}>
                  {r.name}
                </div>
                {r.description && (
                  <div className="text-xs text-slate-500 mt-0.5 truncate">{r.description}</div>
                )}
              </li>
            );
          })}
        </ul>
      )}
    </div>
  </div>
);

export const AssignRolesModal = ({ user, realmRoles, loading, onClose, onSubmit }: Props) => {
  const [initial, setInitial] = useState<string[] | null>(null);
  const [assigned, setAssigned] = useState<Set<string>>(new Set());
  const [availableSelected, setAvailableSelected] = useState<Set<string>>(new Set());
  const [assignedSelected, setAssignedSelected] = useState<Set<string>>(new Set());
  const [availableSearch, setAvailableSearch] = useState('');
  const [assignedSearch, setAssignedSearch] = useState('');

  useEffect(() => {
    let cancelled = false;
    fetchUserRolesApi(user.id)
      .then(roles => {
        if (cancelled) return;
        setInitial(roles);
        setAssigned(new Set(roles));
      })
      .catch(() => {
        if (cancelled) return;
        toast.error("Failed to load this user's current roles.");
        onClose();
      });
    return () => {
      cancelled = true;
    };
  }, [user.id, onClose]);

  const { availableRoles, assignedRoles } = useMemo(() => {
    const available: RealmRole[] = [];
    const inAssigned: RealmRole[] = [];
    for (const r of realmRoles) {
      if (assigned.has(r.name)) inAssigned.push(r);
      else available.push(r);
    }
    const filter = (list: RealmRole[], q: string) =>
      q ? list.filter(r => r.name.toLowerCase().includes(q.toLowerCase())) : list;
    return {
      availableRoles: filter(available, availableSearch),
      assignedRoles: filter(inAssigned, assignedSearch),
    };
  }, [realmRoles, assigned, availableSearch, assignedSearch]);

  const toggle = (set: Set<string>, setSet: (s: Set<string>) => void, name: string) => {
    const next = new Set(set);
    if (next.has(name)) next.delete(name);
    else next.add(name);
    setSet(next);
  };

  const moveToAssigned = (names: string[]) => {
    if (names.length === 0) return;
    const next = new Set(assigned);
    names.forEach(n => next.add(n));
    setAssigned(next);
    setAvailableSelected(new Set());
  };

  const moveToAvailable = (names: string[]) => {
    if (names.length === 0) return;
    const next = new Set(assigned);
    names.forEach(n => next.delete(n));
    setAssigned(next);
    setAssignedSelected(new Set());
  };

  const assignSelected = () => moveToAssigned([...availableSelected]);
  const unassignSelected = () => moveToAvailable([...assignedSelected]);
  const assignAll = () => moveToAssigned(availableRoles.map(r => r.name));
  const unassignAll = () => moveToAvailable(assignedRoles.map(r => r.name));

  const handleSubmit = async () => {
    const err = await onSubmit({ roles: [...assigned] });
    if (!err) onClose();
  };

  const dirty =
    initial !== null &&
    (assigned.size !== initial.length || initial.some(r => !assigned.has(r)));

  return (
    <Modal title="Manage Roles" onClose={onClose} size="3xl">
      <div className="space-y-4">
        <div className="bg-slate-50 rounded-lg px-3 py-2 text-xs text-slate-500 flex items-center justify-between">
          <span>
            User: <span className="font-mono font-semibold text-slate-700">{user.username}</span>
          </span>
          <span className="text-slate-400">
            {initial === null
              ? 'Loading current roles…'
              : `${assigned.size} role${assigned.size === 1 ? '' : 's'} assigned`}
          </span>
        </div>

        {initial === null && (
          <div className="flex items-center justify-center h-72 text-slate-400">
            <Loader2 className="w-5 h-5 animate-spin" />
          </div>
        )}

        {initial !== null && (
        <div className="flex items-stretch gap-3">
          <RoleList
            title="Available Roles"
            roles={availableRoles}
            selected={availableSelected}
            onToggle={name => toggle(availableSelected, setAvailableSelected, name)}
            onMove={name => moveToAssigned([name])}
            search={availableSearch}
            onSearchChange={setAvailableSearch}
            emptyText={availableSearch ? 'No matches' : 'All roles assigned'}
          />

          <div className="flex flex-col justify-center gap-2">
            <button
              type="button"
              onClick={assignSelected}
              disabled={availableSelected.size === 0}
              title="Assign selected"
              className="p-2 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-200 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronRight className="w-4 h-4" />
            </button>
            <button
              type="button"
              onClick={assignAll}
              disabled={availableRoles.length === 0}
              title="Assign all"
              className="p-2 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-200 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronsRight className="w-4 h-4" />
            </button>
            <button
              type="button"
              onClick={unassignSelected}
              disabled={assignedSelected.size === 0}
              title="Unassign selected"
              className="p-2 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-rose-50 hover:text-rose-600 hover:border-rose-200 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronLeft className="w-4 h-4" />
            </button>
            <button
              type="button"
              onClick={unassignAll}
              disabled={assignedRoles.length === 0}
              title="Unassign all"
              className="p-2 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-rose-50 hover:text-rose-600 hover:border-rose-200 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronsLeft className="w-4 h-4" />
            </button>
          </div>

          <RoleList
            title="Assigned Roles"
            roles={assignedRoles}
            selected={assignedSelected}
            onToggle={name => toggle(assignedSelected, setAssignedSelected, name)}
            onMove={name => moveToAvailable([name])}
            search={assignedSearch}
            onSearchChange={setAssignedSearch}
            emptyText={assignedSearch ? 'No matches' : 'No roles assigned'}
          />
        </div>
        )}

        {initial !== null && (
          <p className="text-xs text-slate-400">
            Tip: click a role to select it, then use the arrows. Double-click to move it instantly.
          </p>
        )}
      </div>

      <div className="flex justify-end gap-2 mt-6 pt-5 border-t border-slate-100">
        <button
          onClick={onClose}
          className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors"
        >
          Cancel
        </button>
        <button
          onClick={handleSubmit}
          disabled={loading || !dirty}
          className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          Save Roles
        </button>
      </div>
    </Modal>
  );
};
