import React, { FunctionComponent } from 'react';
import { REACT_APP_BACKEND_URL } from '../../../../config';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

export const HomeAction: FunctionComponent = () => {
  return (
    <a href={`${REACT_APP_BACKEND_URL}/login/sorface?redirectUri=${encodeURIComponent(window.location.href)}`}>
      <button>{useLocalizationCaptions()[LocalizationKey.Login]}</button>
    </a>
  );
};
