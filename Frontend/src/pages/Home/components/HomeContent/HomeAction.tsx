import React, { FunctionComponent } from 'react';
import { REACT_APP_BACKEND_URL } from '../../../../config';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';

export const HomeAction: FunctionComponent = () => {
  return (
    <a href={`${REACT_APP_BACKEND_URL}/login/sorface?redirectUri=${encodeURIComponent(window.location.href)}`}>
      <Button>{useLocalizationCaptions()[LocalizationKey.Login]}</Button>
    </a>
  );
};
