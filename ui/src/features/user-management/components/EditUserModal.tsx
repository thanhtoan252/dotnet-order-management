import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import { FormField, inputCls } from '../../../components/FormField';
import type { KeycloakUser, UpdateUserRequest } from '../types';

interface Props {
  user: KeycloakUser;
  loading: boolean;
  onClose: () => void;
  onSubmit: (form: UpdateUserRequest) => Promise<string | null>;
}

export const EditUserModal = ({ user, loading, onClose, onSubmit }: Props) => {
  const [form, setForm] = useState<UpdateUserRequest>({
    email: user.email ?? '',
    firstName: user.firstName ?? '',
    lastName: user.lastName ?? '',
    enabled: user.enabled,
  });
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (form.email && !/^\S+@\S+\.\S+$/.test(form.email)) {
      setError('Email must be a valid address.');
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
    <Modal title="Edit User" onClose={onClose}>
      <div className="space-y-4">
        {error && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        )}
        <div className="bg-slate-50 rounded-lg px-3 py-2 text-xs text-slate-500">
          Username: <span className="font-mono font-semibold text-slate-700">{user.username}</span>
          <span className="ml-2 text-slate-400">(immutable)</span>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <FormField label="First Name">
            <input
              className={inputCls}
              value={form.firstName ?? ''}
              onChange={e => setForm(f => ({ ...f, firstName: e.target.value }))}
              autoFocus
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
        <label className="flex items-center gap-2 text-sm text-slate-700">
          <input
            type="checkbox"
            checked={form.enabled}
            onChange={e => setForm(f => ({ ...f, enabled: e.target.checked }))}
            className="w-4 h-4"
          />
          Enabled — user can log in
        </label>
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
          Save Changes
        </button>
      </div>
    </Modal>
  );
};
