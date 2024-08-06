import { FunctionComponent } from 'react';
import { IconNames } from '../../../../constants';

import './Icon.css';

interface ThemedIconProps {
  name: IconNames;
}

export const Icon: FunctionComponent<ThemedIconProps> = ({
  name,
}) => {
  return (
    <div className='icon' role='img'>
      <div className='icon-inner'>
        <svg><use href={`/icons-spritesheet.svg#${name}`} /></svg>
      </div>
    </div>
  );
};
