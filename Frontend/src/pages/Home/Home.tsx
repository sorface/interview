import React, { FunctionComponent, useContext } from 'react';
import { Link, Navigate, useParams } from 'react-router-dom';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { HomeAction } from './components/HomeContent/HomeAction';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './Home.css';

export const Home: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { redirect } = useParams();
  const localizationCaptions = useLocalizationCaptions();

  if (auth && redirect) {
    return <Navigate to={redirect} replace />;
  }

  if (auth) {
    return <Navigate to={pathnames.currentRooms} replace />;
  }

  return (
    <MainContentWrapper>
      <Field>
        <p>{localizationCaptions[LocalizationKey.AppDescription]}</p>
        <div className="home-action">
          <HomeAction />
        </div>
        <Link
          className="home-terms-link"
          to={pathnames.terms}
        >
          {localizationCaptions[LocalizationKey.TermsOfUsage]}
        </Link>
      </Field>
    </MainContentWrapper>
  );
};
