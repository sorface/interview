import { FunctionComponent } from 'react';
import { IconNames } from '../../../../constants';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';

import './Icon.css';

export interface IconProps {
  name: IconNames;
  size?: 'xxl' | 'm' | 's';
  secondary?: boolean;
}

const getSizeClassName = (size: IconProps['size']) => {
  switch (size) {
    case 's':
      return 'w-1.125 h-1.125';
    case 'm':
      return 'w-1.375 h-1.375';
    case 'xxl':
      return 'w-4 h-4';
    default:
      return '';
  }
};

export const Icon: FunctionComponent<IconProps> = ({
  name,
  size,
  secondary,
}) => {
  const sizeClassName = getSizeClassName(size);
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
