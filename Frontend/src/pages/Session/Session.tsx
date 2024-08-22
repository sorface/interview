import React, { FunctionComponent, useCallback, useContext, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { AuthContext } from '../../context/AuthContext';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { useLogout } from '../../hooks/useLogout';
import { ThemeSwitch } from '../../components/ThemeSwitch/ThemeSwitch';
import { LangSwitch } from '../../components/LangSwitch/LangSwitch';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';
import { pathnames } from '../../constants';

import './Session.css';

export const Session: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const navigate = useNavigate();
  const { logout, logoutState: { process: { logoutCode } } } = useLogout();
  const localizationCaptions = useLocalizationCaptions();

  useEffect(() => {
    if (!logoutCode || logoutCode !== 200) {
      return;
    }
    navigate(pathnames.home.replace(':redirect?', ''));
  }, [logoutCode, navigate]);

  const handleLogOut = useCallback(() => {
    logout();
  }, [logout]);

  const renderAuth = useCallback(() => {
    if (!auth) {
      return (
        <Field>
          <div>I have nothing to show you... What the heck</div>
        </Field>
      );
    }
    return (
      <>
        <Field className="session-info">
          <ThemeSwitch />
        </Field>
        <Field className="session-info">
          <LangSwitch />
        </Field>
        <Field>
          <Button variant='danger' onClick={handleLogOut}>{localizationCaptions[LocalizationKey.LogOut]}</Button>
        </Field>
      </>
    );
  }, [auth, localizationCaptions, handleLogOut]);

  return (
    <MainContentWrapper>
      <PageHeader
        title={localizationCaptions[LocalizationKey.Settings]}
      />
      {renderAuth()}
    </MainContentWrapper>
  );
};
