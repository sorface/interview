import React, { ReactNode, FunctionComponent } from 'react';

import './FieldsBlock.css';

interface FieldsBlockProps {
  className?: string;
  children: ReactNode;
}

export const FieldsBlock: FunctionComponent<FieldsBlockProps> = ({ children, className }) => (
  <div className={`fields-block ${className || ''}`}>
    <div className="fields-wrap">{children}</div>
  </div>
);
