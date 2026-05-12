import { useRef, useState } from 'react';
import { AlertTriangle, Download, FileSpreadsheet, Upload } from 'lucide-react';
import { Modal } from '../../../components/Modal';

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const XLSX_MIME = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

interface Props {
  loading: boolean;
  onClose: () => void;
  onImport: (file: File) => Promise<Record<string, string[]> | null>;
}

interface RowError {
  rowNumber: number;
  field: string;
  message: string;
}

function parseRowErrors(errors: Record<string, string[]>): RowError[] {
  const result: RowError[] = [];
  for (const [key, messages] of Object.entries(errors)) {
    const match = key.match(/^row\[(\d+)\]\.(.+)$/);
    if (match) {
      result.push({ rowNumber: parseInt(match[1], 10), field: match[2], message: messages[0] });
    }
  }
  return result.sort((a, b) => a.rowNumber - b.rowNumber || a.field.localeCompare(b.field));
}

function getFileErrors(errors: Record<string, string[]>): string[] {
  return errors['file'] ?? [];
}

export const ImportProductsModal = ({ loading, onClose, onImport }: Props) => {
  const inputRef = useRef<HTMLInputElement>(null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [clientError, setClientError] = useState<string | null>(null);
  const [serverErrors, setServerErrors] = useState<Record<string, string[]> | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    setServerErrors(null);
    setClientError(null);

    if (!file) {
      setSelectedFile(null);
      return;
    }

    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      setClientError('Only .xlsx files are accepted.');
      setSelectedFile(null);
      return;
    }

    if (file.size > MAX_FILE_SIZE) {
      setClientError('File exceeds the 5 MB limit.');
      setSelectedFile(null);
      return;
    }

    setSelectedFile(file);
  };

  const handleImport = async () => {
    if (!selectedFile) return;

    const rowErrors = await onImport(selectedFile);
    if (rowErrors === null) {
      onClose();
      return;
    }

    setServerErrors(rowErrors);
  };

  const rowErrors = serverErrors ? parseRowErrors(serverErrors) : [];
  const fileErrors = serverErrors ? getFileErrors(serverErrors) : [];
  const hasErrors = rowErrors.length > 0 || fileErrors.length > 0;

  return (
    <Modal title="Import Products" onClose={onClose}>
      <div className="space-y-4">
        <p className="text-sm text-slate-500">
          Upload an <span className="font-medium text-slate-700">.xlsx</span> file with columns:{' '}
          <span className="font-mono text-xs bg-slate-100 px-1 py-0.5 rounded">
            Name, SKU, Price, Currency, Description, InitialStockQuantity
          </span>
          . Max 5 MB.
        </p>

        <a
          href="/products-template.xlsx"
          download
          className="inline-flex items-center gap-1.5 text-xs text-indigo-600 hover:text-indigo-700 font-medium"
        >
          <Download className="w-3.5 h-3.5" />
          Download template
        </a>

        <div
          className="border-2 border-dashed border-slate-200 rounded-lg p-6 text-center cursor-pointer hover:border-indigo-400 hover:bg-indigo-50/40 transition-colors"
          onClick={() => inputRef.current?.click()}
        >
          <input
            ref={inputRef}
            type="file"
            accept={`.xlsx,${XLSX_MIME}`}
            className="hidden"
            onChange={handleFileChange}
          />
          {selectedFile ? (
            <div className="flex items-center justify-center gap-2 text-sm text-slate-700">
              <FileSpreadsheet className="w-5 h-5 text-green-600 flex-shrink-0" />
              <span className="font-medium">{selectedFile.name}</span>
              <span className="text-slate-400">({(selectedFile.size / 1024).toFixed(1)} KB)</span>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-2 text-slate-400">
              <Upload className="w-8 h-8" />
              <span className="text-sm">Click to select a file</span>
            </div>
          )}
        </div>

        {clientError && (
          <div className="flex items-center gap-2 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2.5 rounded-lg">
            <AlertTriangle className="w-4 h-4 flex-shrink-0" />
            {clientError}
          </div>
        )}

        {hasErrors && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-3">
            {fileErrors.map((msg, i) => (
              <p key={i} className="text-sm text-red-700 font-medium mb-1">
                {msg}
              </p>
            ))}
            {rowErrors.length > 0 && (
              <>
                <p className="text-sm font-medium text-red-700 mb-2">
                  {rowErrors.length} row error{rowErrors.length !== 1 ? 's' : ''} found — fix and re-upload:
                </p>
                <ul className="max-h-48 overflow-y-auto space-y-1">
                  {rowErrors.map((err, i) => (
                    <li key={i} className="text-xs text-red-600">
                      <span className="font-semibold">Row {err.rowNumber}</span> &mdash;{' '}
                      <span className="capitalize">{err.field}</span>: {err.message}
                    </li>
                  ))}
                </ul>
              </>
            )}
          </div>
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
          onClick={handleImport}
          disabled={loading || !selectedFile}
          className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          <Upload className="w-4 h-4" />
          {loading ? 'Importing…' : 'Import'}
        </button>
      </div>
    </Modal>
  );
};
