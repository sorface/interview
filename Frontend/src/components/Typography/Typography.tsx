import { FunctionComponent, ReactNode } from 'react';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

import './Typography.css';

export interface TypographyProps {
  size: 'xxl' | 'xl' | 'l' | 'm' | 's' | 'xs';
  bold?: boolean;
  error?: boolean;
  children: ReactNode;
}

export const Typography: FunctionComponent<TypographyProps> = ({
  size,
  bold,
  error,
  children,
}) => {
  const errorClassName = useThemeClassName({
    [Theme.Dark]: 'text-dark-orange',
    [Theme.Light]: 'text-red',
  });

  return (
    <span className={`typography typography-${size} ${bold ? 'typography-bold' : ''} ${error ? errorClassName : ''}`}>
      {children}
    </span>
  );
};
