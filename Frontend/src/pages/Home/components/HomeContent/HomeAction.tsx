import React, { FunctionComponent } from 'react';
import { VITE_BACKEND_URL } from '../../../../config';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';
import { Icon } from '../../../Room/components/Icon/Icon';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';

export const HomeAction: FunctionComponent = () => {
  return (
    <a
      href={`${VITE_BACKEND_URL}/login/sorface?redirectUri=${encodeURIComponent(window.location.href)}`}
    >
      <Button variant='invertedActive'>
        {useLocalizationCaptions()[LocalizationKey.Login]}
        <Gap horizontal sizeRem={0.15} />
        <Icon name={IconNames.ChevronForward} />
      </Button>
    </a>
  );
};
