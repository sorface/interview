import React, { FunctionComponent, ReactNode } from 'react';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

import './Typography.css';

export interface TypographyProps {
  size:
    | 'xxxl'
    | 'xxl'
    | 'xl'
    | 'l'
    | 'm'
    | 's'
    | 'xs'
    | 'landing-l'
    | 'landing-m'
    | 'roadmaps-heading';
  bold?: boolean;
  semibold?: boolean;
  error?: boolean;
  success?: boolean;
  secondary?: boolean;
  children: ReactNode;
}

export const Typography: FunctionComponent<TypographyProps> = ({
  size,
  bold,
  semibold,
  error,
  success,
  secondary,
  children,
}) => {
  const errorClassName = useThemeClassName({
    [Theme.Dark]: 'text-dark-orange',
    [Theme.Light]: 'text-red',
  });

  const successClassName = useThemeClassName({
    [Theme.Dark]: 'text-green',
    [Theme.Light]: 'text-dark-green',
  });

  const secondaryClassName = useThemeClassName({
    [Theme.Dark]: 'text-grey3',
    [Theme.Light]: 'text-grey3',
  });

  return (
    <span
      className={`typography typography-${size} ${bold ? 'typography-bold' : ''} ${semibold ? 'typography-semibold' : ''} ${error ? errorClassName : ''} ${secondary ? secondaryClassName : ''} ${success ? successClassName : ''}`}
    >
      {children}
    </span>
  );
};
