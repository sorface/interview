import React, { FunctionComponent } from 'react';
import { IconNames } from '../../../../constants';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';

import iconsSpritesheet from './icons-spritesheet.svg';
import './Icon.css';

export interface IconProps {
  name: IconNames;
  size?: 'xxl' | 'm' | 'l' | 's';
  inheritFontSize?: boolean;
  secondary?: boolean;
  danger?: boolean;
}

const getSizeClassName = (size: IconProps['size']) => {
  switch (size) {
    case 's':
      return 'w-[1.125rem] h-[1.125rem]';
    case 'm':
      return 'w-[1.25rem] h-[1.25rem]';
    case 'l':
      return 'w-[1.375rem] h-[1.375rem]';
    case 'xxl':
      return 'w-[4rem] h-[4rem]';
    default:
      return '';
  }
};

export const Icon: FunctionComponent<IconProps> = ({
  name,
  size,
  inheritFontSize,
  secondary,
  danger,
}) => {
  const sizeClassName = inheritFontSize ? '' : getSizeClassName(size || 'm');
  const themeSecondaryClassName = useThemeClassName({
    [Theme.Dark]: 'text-dark-grey4',
    [Theme.Light]: 'text-grey3',
  });
  const secondaryClassName = secondary ? themeSecondaryClassName : '';
  const themeDangerClassName = useThemeClassName({
    [Theme.Dark]: '',
    [Theme.Light]: 'text-red',
  });
  const dangerClassName = danger ? themeDangerClassName : '';

  return (
    <div
      className={`icon ${sizeClassName} ${secondaryClassName} ${dangerClassName}`}
      role="img"
    >
      <div className="icon-inner">
        <svg>
          <use href={`${iconsSpritesheet}#${name}`} />
        </svg>
      </div>
    </div>
  );
};
