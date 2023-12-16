import React, { ReactNode, FunctionComponent } from 'react';

interface FieldProps {
  className?: string;
  children: ReactNode;
}

export const Field: FunctionComponent<FieldProps> = ({ children, className }) => (
  <>
    <div className={`field-wrap ${className || ''}`}>{children}</div>
  </>
);
