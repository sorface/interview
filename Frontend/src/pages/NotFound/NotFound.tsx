import React, { FunctionComponent } from 'react';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';

export const NotFound: FunctionComponent = () => {
  return (
    <MainContentWrapper>
      <Field>
        <div>NotFound</div>
      </Field>
    </MainContentWrapper>
  );
};
