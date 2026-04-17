import { useState, useEffect, useRef, useId } from 'react';
import { X } from 'lucide-react';

interface ModalProps {
  title: string;
  onClose: () => void;
  children: React.ReactNode;
}

export function Modal({ title, onClose, children }: ModalProps) {
  const titleId = useId();
  const [pos, setPos] = useState({ x: 0, y: 0 });
  const dragging = useRef(false);
  const startRef = useRef({ mx: 0, my: 0, x: 0, y: 0 });

  const onMouseDown = (e: React.MouseEvent) => {
    dragging.current = true;
    startRef.current = { mx: e.clientX, my: e.clientY, x: pos.x, y: pos.y };
    e.preventDefault();
  };

  useEffect(() => {
    const onMouseMove = (e: MouseEvent) => {
      if (!dragging.current) return;
      setPos({
        x: startRef.current.x + e.clientX - startRef.current.mx,
        y: startRef.current.y + e.clientY - startRef.current.my,
      });
    };
    const onMouseUp = () => { dragging.current = false; };
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
    window.addEventListener('keydown', onKeyDown);
    return () => {
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
      window.removeEventListener('keydown', onKeyDown);
    };
  }, [onClose]);

  return (
    <div
      className="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby={titleId}
    >
      <div
        className="bg-white rounded-2xl shadow-2xl w-full max-w-md border border-slate-200"
        style={{ transform: `translate(${pos.x}px, ${pos.y}px)` }}
      >
        <div
          className="flex items-center justify-between px-6 py-4 border-b border-slate-100 cursor-grab active:cursor-grabbing select-none"
          onMouseDown={onMouseDown}
        >
          <h2 id={titleId} className="text-base font-semibold text-slate-900">{title}</h2>
          <button
            onClick={onClose}
            onMouseDown={e => e.stopPropagation()}
            aria-label="Close"
            className="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
        <div className="px-6 py-5">{children}</div>
      </div>
    </div>
  );
}
