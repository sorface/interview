import React, { FunctionComponent, useCallback, useContext } from 'react';
import { AuthContext } from '../../context/AuthContext';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { useCommunist } from '../../hooks/useCommunist';
import { ThemeSwitch } from '../../components/ThemeSwitch/ThemeSwitch';
import { Localization } from '../../localization';
import { HeaderField } from '../../components/HeaderField/HeaderField';

import './Session.css';

export const Session: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { resetCommunist } = useCommunist();

  const handleLogOut = useCallback(() => {
    resetCommunist();
  }, [resetCommunist]);

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
        <Field>
          <button className="danger" onClick={handleLogOut}>{Localization.LogOut}</button>
        </Field>
      </>
    );
  }, [auth, handleLogOut]);

  return (
    <MainContentWrapper>
      <HeaderField><h2>{Localization.Settings}</h2></HeaderField>
      {renderAuth()}
    </MainContentWrapper>
  );
};
