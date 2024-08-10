import { FunctionComponent } from 'react';
import { IconNames } from '../../../../constants';

import './Icon.css';

interface ThemedIconProps {
  name: IconNames;
  size?: 'm' | 's';
}

export const Icon: FunctionComponent<ThemedIconProps> = ({
  name,
  size,
}) => {
  const sizeClassName = size === 's' ? 'w-1.125 h-1.125' : 'w-1.375 h-1.375';
  return (
    <div className={`icon ${sizeClassName}`} role='img'>
      <div className='icon-inner'>
        <svg><use href={`/icons-spritesheet.svg#${name}`} /></svg>
      </div>
    </div>
  );
};
