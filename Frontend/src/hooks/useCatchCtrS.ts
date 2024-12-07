import { useEffect } from 'react';

export const useCatchCtrS = () => {
  const callback = (e: KeyboardEvent) => {
    if (e.key === 's' && (navigator.userAgent.includes('Mac') ? e.metaKey : e.ctrlKey)) {
      e.preventDefault();
    }
  }

  useEffect(() => {
    document.addEventListener("keydown", callback, false);

    return () => {
      document.removeEventListener("keydown", callback, false);
    };
  });
};
