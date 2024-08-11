import React, { ReactNode, FunctionComponent } from 'react';

import './FieldsBlock.css';

interface FieldsBlockProps {
  className?: string;
  withMargin?: boolean;
  children: ReactNode;
}

export const FieldsBlock: FunctionComponent<FieldsBlockProps> = ({ children, withMargin, className }) => (
  <div className={`fields-block ${className || ''} ${withMargin ? 'with-margin' : ''}`}>
    <div className="fields-wrap flex flex-col overflow-auto">{children}</div>
  </div>
);
