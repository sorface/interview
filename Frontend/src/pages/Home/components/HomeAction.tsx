import React, { FunctionComponent, useContext } from 'react';
import { VITE_BACKEND_URL } from '../../../config';
import { LocalizationKey } from '../../../localization';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { Gap } from '../../../components/Gap/Gap';
import { LoginButton } from './LoginButton';
import { setDevAuthorization } from '../../../utils/devAuthorization';
import { DeviceContext } from '../../../context/DeviceContext';

export const HomeAction: FunctionComponent = () => {
  const device = useContext(DeviceContext);
  const loginCaption = useLocalizationCaptions()[LocalizationKey.Login];

  const handleLogin = (authorization: string) => () => {
    setDevAuthorization(authorization);
    location.reload();
  };

  if (import.meta.env.MODE === 'development') {
    return (
      <div
        className={`flex justify-center ${device === 'Mobile' ? 'w-full' : ''}`}
      >
        <LoginButton
          caption={`${loginCaption} (User)`}
          onClick={handleLogin('UserDev User')}
        />
        <Gap sizeRem={0.5} horizontal />
        <LoginButton
          caption={`${loginCaption} (Admin)`}
          onClick={handleLogin('AdminDev User_Admin')}
        />
      </div>
    );
  }

  return (
    <div className="w-full">
      <a
        href={`${VITE_BACKEND_URL}/login/sorface?redirectUri=${encodeURIComponent(window.location.href)}`}
      >
        <LoginButton caption={loginCaption} />
      </a>
    </div>
  );
};
