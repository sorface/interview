import React, { FunctionComponent, Fragment, useContext } from 'react';
import terms from './terms.json';
import { pathnames } from '../../constants';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Typography } from '../../components/Typography/Typography';
import { Gap } from '../../components/Gap/Gap';
import { LocalizationContext } from '../../context/LocalizationContext';

interface Term {
  title: string;
  description: string;
}

export const Terms: FunctionComponent = () => {
  const { lang } = useContext(LocalizationContext);
  const localizationCaptions = useLocalizationCaptions();

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
      <Typography size='xxl' bold>
        {`${index + 1}. ${term.title}`}
      </Typography>
      <Gap sizeRem={1} />
      <Typography size='m'>
        {interpolateAll(
          term.description,
          [/\[NAME\]/g, /\[NAME URL\]/g],
          [localizationCaptions[LocalizationKey.AppName], termsUrl],
        )}
      </Typography>
      <Gap sizeRem={2} />
    </Fragment>
  );

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.AppName]}
      />
      <div className='text-left flex flex-col overflow-auto'>
        <Typography size='xxl' bold>
          {localizationCaptions[LocalizationKey.TermsOfUsage]}
        </Typography>
        <Gap sizeRem={1} />
        <Typography size='m'>
          {terms[lang].disclaimer}
        </Typography>
        <Gap sizeRem={2} />
        {terms[lang].items.map(renderTerm)}
      </div>
    </>
  );
};
