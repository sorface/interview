import { FunctionComponent } from 'react';
import { IconNames } from '../../../../constants';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';

import './Icon.css';

export interface IconProps {
  name: IconNames;
  size?: 'm' | 's';
  secondary?: boolean;
}

export const Icon: FunctionComponent<IconProps> = ({
  name,
  size,
  secondary,
}) => {
  const sizeClassName = size === 's' ? 'w-1.125 h-1.125' : 'w-1.375 h-1.375';
  const themeSecondaryClassName = useThemeClassName({
    [Theme.Dark]: 'text-dark-grey4',
    [Theme.Light]: 'text-grey3',
  });
  const secondaryClassName = secondary ? themeSecondaryClassName : '';

  return (
    <div className={`icon ${sizeClassName} ${secondaryClassName}`} role='img'>
      <div className='icon-inner'>
        <svg><use href={`/icons-spritesheet.svg#${name}`} /></svg>
      </div>
    </div>
  );
};
