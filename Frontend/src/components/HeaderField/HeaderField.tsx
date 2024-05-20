import React, { FunctionComponent, ReactNode } from 'react';
import { Field } from '../FieldsBlock/Field';

import './HeaderField.css';

interface HeaderFieldProps {
  title?: string;
  children?: ReactNode;
}

export const HeaderField: FunctionComponent<HeaderFieldProps> = ({
  title,
  children,
}) => {
  return (
    <Field className="header-field">
      {!!title && <span>{title}</span>}
      {!!children && children}
    </Field>
  );
};
