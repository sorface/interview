import React, { FunctionComponent, useContext } from 'react';
import { Link, Navigate, useParams } from 'react-router-dom';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { HomeAction } from './components/HomeContent/HomeAction';
import { Localization } from '../../localization';
import { HeaderField } from '../../components/HeaderField/HeaderField';

import './Home.css';

export const Home: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { redirect } = useParams();

  if (auth && redirect) {
    return <Navigate to={redirect} replace />;
  }

  if (auth) {
    return <Navigate to={pathnames.rooms} replace />;
  }

  return (
    <MainContentWrapper>
      <HeaderField/>
      <Field>
        <p>{Localization.AppDescription}</p>
        <div className="home-action">
          <HomeAction />
        </div>
        <Link
          className="home-terms-link"
          to={pathnames.terms}
        >
          {Localization.TermsOfUsage}
        </Link>
      </Field>
    </MainContentWrapper>
  );
};
