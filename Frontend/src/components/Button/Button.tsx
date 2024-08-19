import { FunctionComponent, ReactNode } from 'react';

import './Button.css';

type Variant =
  'active' |
  'active2' |
  'danger' |
  'inverted' |
  'invertedAlternative' |
  'invertedActive' |
  'text' |
  'toolsPanel' |
  'toolsPanelDanger';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
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
      className={`${variant || ''} ${rest.className || ''}`}
    >
      {children}
    </button>
  );
};
