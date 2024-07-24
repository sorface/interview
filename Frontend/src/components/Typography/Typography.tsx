import { FunctionComponent, ReactNode } from 'react';

import './Typography.css';

export interface TypographyProps {
  size: 'xl' | 'l' | 'm' | 's';
  bold?: boolean;
  children: ReactNode;
}

export const Typography: FunctionComponent<TypographyProps> = ({
  size,
  bold,
  children,
}) => (
  <span className={`typography typography-${size} ${bold ? 'typography-bold' : ''}`}>
    {children}
  </span>
);
