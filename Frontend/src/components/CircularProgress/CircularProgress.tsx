import { FunctionComponent } from 'react';

import './CircularProgress.css';
import { Typography } from '../Typography/Typography';

interface CircularProgressProps {
  value: number;
  caption: string | number;
  size: 'm' | 's';
}

export const CircularProgress: FunctionComponent<CircularProgressProps> = ({
  value,
  caption,
  size,
}) => {
  const sizePx = size === 'm' ? 108 : 40;
  const strokeWidth = size === 'm' ? '12px' : '4px';
  return (
    <div
      className='relative circular-progress-wrapper'
      style={{ '--size': `${sizePx}px` } as React.CSSProperties}
    >
      <svg
        width={`${sizePx}`}
        height={`${sizePx}`}
        viewBox={`0 0 ${sizePx} ${sizePx}`}
        className='circular-progress'
        style={{
          '--end-progress': value,
          '--stroke-width': strokeWidth,
        } as React.CSSProperties}
      >
        <circle stroke='var(--blue-light)'></circle>
        <circle stroke='var(--blue-main)' className='fg'></circle>
      </svg>
      <div className='circular-progress-caption'>
        <Typography size={size === 'm' ? 'xxxl' : 'm'}>{caption}</Typography>
      </div>
    </div>
  );
};
