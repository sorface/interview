import React, { ReactNode, FunctionComponent } from 'react';
import { FieldsBlock } from '../../components/FieldsBlock/FieldsBlock';

import './MainContentWrapper.css';

interface MainContentWrapperProps {
  thin?: boolean;
  className?: string;
  children: ReactNode;
}

export const MainContentWrapper: FunctionComponent<MainContentWrapperProps> =
  ({ thin, className, children }) => {
    return (
      <FieldsBlock
        className={`${className} ${thin ? 'thin-page-content-wrapper' : ''}`}
      >
        {children}
      </FieldsBlock>
    );
  };
