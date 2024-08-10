import React, { ReactNode, FunctionComponent } from 'react';
import { FieldsBlock } from '../../components/FieldsBlock/FieldsBlock';

import './MainContentWrapper.css';

interface MainContentWrapperProps {
  className?: string;
  withMargin?: boolean;
  children: ReactNode;
}

export const MainContentWrapper: FunctionComponent<MainContentWrapperProps> =
  ({ className, withMargin, children }) => {
    return (
      <FieldsBlock
        withMargin={withMargin}
        className={`${className} h-full`}
      >
        {children}
      </FieldsBlock>
    );
  };
