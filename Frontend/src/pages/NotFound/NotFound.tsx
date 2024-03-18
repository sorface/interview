import React, { FunctionComponent } from 'react';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { HeaderField } from '../../components/HeaderField/HeaderField';

export const NotFound: FunctionComponent = () => {
  return (
    <MainContentWrapper>
      <HeaderField/>
      <Field>
        <div>NotFound</div>
      </Field>
    </MainContentWrapper>
  );
};
