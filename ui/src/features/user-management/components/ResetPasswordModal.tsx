import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { Modal } from '../../../components/Modal';
import { FormField, inputCls } from '../../../components/FormField';
import type { KeycloakUser, ResetPasswordRequest } from '../types';

interface Props {
  user: KeycloakUser;
  loading: boolean;
  onClose: () => void;
  onSubmit: (req: ResetPasswordRequest) => Promise<string | null>;
}

export const ResetPasswordModal = ({ user, loading, onClose, onSubmit }: Props) => {
  const [password, setPassword] = useState('');
  const [temporary, setTemporary] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async () => {
    if (password.length < 6) {
      setError('Password must be at least 6 characters.');
      return;
    }
    const err = await onSubmit({ password, temporary });
    if (!err) {
      onClose();
    }
  };

  return (
    <Modal title="Reset Password" onClose={onClose}>
      <div className="space-y-4">
        {error && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {error}
          </div>
        )}
        <div className="bg-slate-50 rounded-lg px-3 py-2 text-xs text-slate-500">
          User: <span className="font-mono font-semibold text-slate-700">{user.username}</span>
        </div>
        <FormField label="New Password *">
          <input
            className={inputCls}
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            autoFocus
          />
        </FormField>
        <label className="flex items-center gap-2 text-sm text-slate-700">
          <input
            type="checkbox"
            checked={temporary}
            onChange={e => setTemporary(e.target.checked)}
            className="w-4 h-4"
          />
          Temporary — user must change on next login
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
          className="px-4 py-2 text-sm font-medium text-white bg-amber-600 rounded-lg hover:bg-amber-700 disabled:opacity-50 transition-colors"
        >
          Reset Password
        </button>
      </div>
    </Modal>
  );
};
