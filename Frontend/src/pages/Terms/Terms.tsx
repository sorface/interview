import React, { FunctionComponent, Fragment } from 'react';
import terms from './terms.json';
import { pathnames } from '../../constants';
import { FieldsBlock } from '../../components/FieldsBlock/FieldsBlock';
import { Field } from '../../components/FieldsBlock/Field';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { Localization } from '../../localization';

import './Terms.css';

interface Term {
  title: string;
  description: string;
}

export const Terms: FunctionComponent = () => {
  const interpolate =
    (text: string, searchRegExp: RegExp, replaceWith: string) =>
      text.replace(searchRegExp, replaceWith);

  const interpolateAll =
    (text: string, searchRegExps: RegExp[], replaceWith: string[]) =>
      searchRegExps.reduce(
        (accum, currRegExp, index) => interpolate(accum, currRegExp, replaceWith[index]),
        text,
      );

  const termsUrl = `${document.location.origin}${pathnames.terms} `;

  const renderTerm = (term: Term, index: number) => (
    <Fragment key={term.title}>
      <h3>{`${index + 1}. ${term.title}`}</h3>
      <p>
        {interpolateAll(
          term.description,
          [/\[NAME\]/g, /\[NAME URL\]/g],
          [Localization.AppName, termsUrl],
        )}
      </p>
    </Fragment>
  );

  return (
    <FieldsBlock className="terms-of-use">
      <HeaderWithLink
        title={Localization.TermsOfUsage}
        linkVisible={true}
        path={pathnames.home.replace(':redirect?', '')}
        linkCaption="<"
        linkFloat="left"
      />
      <Field>
        <h2>{Localization.TermsOfUsage}</h2>
        <p>{terms.disclaimer}</p>
        {terms.items.map(renderTerm)}
      </Field>
    </FieldsBlock>
  );
};
