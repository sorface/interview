import React, { FunctionComponent } from 'react';
import { Field } from '../../components/FieldsBlock/Field';

import './SubmitField.css';

interface SubmitFieldProps {
  caption: string;
}

export const SubmitField: FunctionComponent<SubmitFieldProps> = ({
  caption,
}) => {
  return (
    <Field className="submit-field">
      <input type="submit" value={caption} />
    </Field>
  );
};
