import { FunctionComponent, ReactNode } from 'react';

import './Button.css';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'active' | 'danger' | 'inverted' | 'text';
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
      className={`${variant || ''} ${rest.className || ''}`}
    >
      {children}
    </button>
  );
};
