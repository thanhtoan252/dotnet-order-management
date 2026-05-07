import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import { FormField, inputCls } from '../../../components/FormField';
import type { CreateUserRequest, RealmRole } from '../types';

interface Props {
  loading: boolean;
  realmRoles: RealmRole[];
  onClose: () => void;
  onSubmit: (form: CreateUserRequest) => Promise<string | null>;
}

const emptyForm: CreateUserRequest = {
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  enabled: true,
  password: '',
  temporaryPassword: true,
  roles: [],
};

export const CreateUserModal = ({ loading, realmRoles, onClose, onSubmit }: Props) => {
  const [form, setForm] = useState<CreateUserRequest>(emptyForm);
  const [error, setError] = useState<string | null>(null);

  const toggleRole = (name: string) => {
    setForm(f => ({
      ...f,
      roles: f.roles.includes(name) ? f.roles.filter(r => r !== name) : [...f.roles, name],
    }));
  };

  const handleSubmit = async () => {
    if (!form.username.trim()) {
      setError('Username is required.');
      return;
    }
    if (form.password.length < 6) {
      setError('Password must be at least 6 characters.');
      return;
    }
    const err = await onSubmit({
      ...form,
      email: form.email?.trim() || undefined,
      firstName: form.firstName?.trim() || undefined,
      lastName: form.lastName?.trim() || undefined,
    });
    if (!err) {
      onClose();
    }
  };

  return (
    <Modal title="New User" onClose={onClose}>
      <div className="space-y-4">
        {error && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        )}
        <FormField label="Username *">
          <input
            className={inputCls}
            placeholder="e.g. jane.doe"
            value={form.username}
            onChange={e => setForm(f => ({ ...f, username: e.target.value }))}
            autoFocus
          />
        </FormField>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="First Name">
            <input
              className={inputCls}
              value={form.firstName ?? ''}
              onChange={e => setForm(f => ({ ...f, firstName: e.target.value }))}
            />
          </FormField>
          <FormField label="Last Name">
            <input
              className={inputCls}
              value={form.lastName ?? ''}
              onChange={e => setForm(f => ({ ...f, lastName: e.target.value }))}
            />
          </FormField>
        </div>
        <FormField label="Email">
          <input
            className={inputCls}
            type="email"
            value={form.email ?? ''}
            onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
          />
        </FormField>
        <FormField label="Password *">
          <input
            className={inputCls}
            type="password"
            value={form.password}
            onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
          />
        </FormField>
        <label className="flex items-center gap-2 text-sm text-slate-700">
          <input
            type="checkbox"
            checked={form.temporaryPassword}
            onChange={e => setForm(f => ({ ...f, temporaryPassword: e.target.checked }))}
            className="w-4 h-4"
          />
          User must change password on first login
        </label>
        <label className="flex items-center gap-2 text-sm text-slate-700">
          <input
            type="checkbox"
            checked={form.enabled}
            onChange={e => setForm(f => ({ ...f, enabled: e.target.checked }))}
            className="w-4 h-4"
          />
          Enabled
        </label>
        <FormField label="Roles">
          <div className="flex flex-wrap gap-2">
            {realmRoles.map(r => (
              <button
                key={r.id}
                type="button"
                onClick={() => toggleRole(r.name)}
                className={[
                  'text-xs px-2.5 py-1 rounded font-medium border transition-colors',
                  form.roles.includes(r.name)
                    ? 'bg-indigo-50 text-indigo-700 border-indigo-200'
                    : 'bg-white text-slate-600 border-slate-200 hover:bg-slate-50',
                ].join(' ')}
              >
                {r.name}
              </button>
            ))}
          </div>
        </FormField>
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
          disabled={loading}
          className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          Create User
        </button>
      </div>
    </Modal>
  );
};
