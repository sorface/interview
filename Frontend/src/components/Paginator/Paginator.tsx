import React, { FunctionComponent, MouseEventHandler } from 'react';
import { Field } from '../FieldsBlock/Field';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './Paginator.css';

interface PaginatorProps {
  pageNumber: number;
  prevDisabled: boolean;
  nextDisabled: boolean;
  onPrevClick: MouseEventHandler<HTMLButtonElement>;
  onNextClick: MouseEventHandler<HTMLButtonElement>;
}

export const Paginator: FunctionComponent<PaginatorProps> = ({
  pageNumber,
  prevDisabled,
  nextDisabled,
  onPrevClick,
  onNextClick,
}) => {
  return (
    <Field className="paginator">
      <button
        type="button"
        disabled={prevDisabled}
        onClick={onPrevClick}
      >
        &#60;
      </button>
      <span>{useLocalizationCaptions()[LocalizationKey.Page]}: {pageNumber}</span>
      <button
        type="button"
        disabled={nextDisabled}
        onClick={onNextClick}
      >
        &#62;
      </button>
    </Field>
  );
};
