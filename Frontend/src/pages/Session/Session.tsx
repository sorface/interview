import React, { FunctionComponent, useContext } from 'react';
import { useLogout } from '../../hooks/useLogout';
import { ThemeSwitch } from '../../components/ThemeSwitch/ThemeSwitch';
import { LangSwitch } from '../../components/LangSwitch/LangSwitch';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';
import { Gap } from '../../components/Gap/Gap';
import { UserAvatar } from '../../components/UserAvatar/UserAvatar';
import { AuthContext } from '../../context/AuthContext';
import { Typography } from '../../components/Typography/Typography';
import { RecognitionLangSwitch } from '../../components/RecognitionLangSwitch/RecognitionLangSwitch';

export const Session: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { logout } = useLogout();
  const localizationCaptions = useLocalizationCaptions();

  const handleLogOut = () => {
    logout();
  };

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.Settings]}
      />
      <div className='flex-1 flex items-center justify-center'>
        <div className='flex-1 flex flex-col items-center'>
          <UserAvatar
            nickname={auth?.nickname || ''}
            src={auth?.avatar}
            size='xl'
          />
          <Gap sizeRem={1} />
          <Typography size='l' bold>{auth?.nickname || ''}</Typography>
          <Gap sizeRem={2} />
          <div className='w-full max-w-29.25 grid grid-cols-settings-list gap-y-1'>
            <ThemeSwitch />
            <LangSwitch />
            <RecognitionLangSwitch />
          </div>
          <Gap sizeRem={2} />
          <Button variant='danger' className='max-w-12' onClick={handleLogOut}>{localizationCaptions[LocalizationKey.LogOut]}</Button>
          <Gap sizeRem={10.625} />
        </div>
      </div>
    </>
  );
};
