import React, { FunctionComponent } from 'react';
import { REACT_APP_BACKEND_URL } from '../../../../config';
import { Localization } from '../../../../localization';

export const HomeAction: FunctionComponent = () => {
  return (
    <a href={`${REACT_APP_BACKEND_URL}/login/twitch?redirectUri=${encodeURIComponent(window.location.href)}`}>
      <button>{Localization.Login}</button>
    </a>
  );
};
