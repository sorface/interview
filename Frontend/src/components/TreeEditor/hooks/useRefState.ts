import React from 'react';
import { uuid } from '../utils/utils';

export function useRefState<T>(initial: T): [T, (value: T) => void] {
  const [renderId, setRenderId] = React.useState(uuid());
  const ref = React.useRef<T>(initial);
  const setValue = React.useCallback(
    (value: T) => {
      ref.current = value;
      setRenderId(uuid());
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [renderId],
  );
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const value: T = React.useMemo(() => ref.current, [renderId]);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  return React.useMemo(() => [value, setValue], [renderId]);
}
