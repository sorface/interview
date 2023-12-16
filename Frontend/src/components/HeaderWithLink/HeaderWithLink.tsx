import React, { FunctionComponent, ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Field } from '../../components/FieldsBlock/Field';

import './HeaderWithLink.css';

interface HeaderWithLinkProps {
  title?: string;
  linkVisible: boolean;
  path: string;
  linkCaption: string;
  linkFloat: 'left' | 'right';
  children?: ReactNode;
}

export const HeaderWithLink: FunctionComponent<HeaderWithLinkProps> = ({
  title,
  linkVisible,
  path,
  linkCaption,
  linkFloat,
  children,
}) => {
  return (
    <Field className="header-with-link">
      {linkVisible && (
        <Link to={path}>
          <button className={`button-link float-${linkFloat}`}>{linkCaption}</button>
        </Link>
      )}
      {!!title && <span>{title}</span>}
      {!!children && children}
    </Field>
  );
};
