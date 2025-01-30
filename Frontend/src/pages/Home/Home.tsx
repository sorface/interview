import React, { FunctionComponent, useContext } from 'react';
import { Link, Navigate, useParams } from 'react-router-dom';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { HomeAction } from './components/HomeContent/HomeAction';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Gap } from '../../components/Gap/Gap';
import { Typography } from '../../components/Typography/Typography';
import { LangSwitch } from '../../components/LangSwitch/LangSwitch';
import { VITE_NAME } from '../../config';

export const Home: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { redirect } = useParams();
  const localizationCaptions = useLocalizationCaptions();

  if (auth && redirect) {
    return <Navigate to={redirect} replace />;
  }

  if (auth) {
    return <Navigate to={pathnames.highlightRooms} replace />;
  }

  return (
    <>
      <PageHeader title={VITE_NAME} />
      <div className="flex flex-1 flex-col justify-center items-center">
        <Typography size="l">
          {localizationCaptions[LocalizationKey.LoginRequired]}
        </Typography>
        <Gap sizeRem={2.25} />
        <HomeAction />
        <Gap sizeRem={0.5} />
        <div>
          <Typography size="s">
            {localizationCaptions[LocalizationKey.TermsOfUsageAcceptance]}
          </Typography>
          <Link to={pathnames.terms}>
            <Typography size="s">
              {localizationCaptions[LocalizationKey.TermsOfUsage]}
            </Typography>
          </Link>
        </div>
      </div>
      <div className="flex justify-center">
        <LangSwitch elementType="switcherButton" />
      </div>
    </>
  );
};
