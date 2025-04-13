import React, { FunctionComponent, ReactNode } from 'react';

import './Button.css';

type Variant =
  | 'active'
  | 'active2'
  | 'danger'
  | 'inverted'
  | 'invertedAlternative'
  | 'invertedActive'
  | 'text'
  | 'toolsPanel'
  | 'toolsPanelAlternative'
  | 'toolsPanelDanger';

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  disabled?: boolean;
  children: ReactNode;
}

export const Button: FunctionComponent<ButtonProps> = ({
  variant,
  disabled,
  children,
  ...rest
}) => {
  return (
    <button
      disabled={disabled}
      {...rest}
      className={`px-[1.25rem] py-[0.5rem] text-[0.875rem] ${variant || ''} ${rest.className || ''}`}
    >
      {children}
    </button>
  );
};
