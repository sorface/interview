import React, { FunctionComponent, useState } from 'react';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Button } from '../Button/Button';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Gap } from '../Gap/Gap';
import { ContextMenu } from '../ContextMenu/ContextMenu';
import { Typography } from '../Typography/Typography';

export const PageHeaderNotifications: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const [open, setOpen] = useState(false);

  const handleOpenSwitch = () => {
    setOpen(!open);
  };

  return (
    <div className="flex items-stretch justify-end h-[2.5rem]">
      <ContextMenu
        translateRem={{ x: -15, y: 0.25 }}
        variant="alternative"
        toggleContent={
          <div className="cursor-pointer">
            <Button
              variant="invertedAlternative"
              className="min-w-[0rem] w-[2.5rem] h-[2.5rem] !p-[0rem]"
              onClick={handleOpenSwitch}
            >
              <Icon size="s" name={IconNames.Notifications} />
            </Button>
          </div>
        }
        contentClassName="w-[17.5rem] rounded-[0.75rem]"
      >
        <div className="flex flex-col py-[1.5rem] px-[1rem]">
          <div className="flex justify-between items-baseline">
            <Typography size="m" bold>
              {localizationCaptions[LocalizationKey.NewNotifications]}
            </Typography>
            <div className="cursor-pointer text-grey3 hover:text-red">
              <Typography size="s">
                {localizationCaptions[LocalizationKey.Clear]}
              </Typography>
            </div>
          </div>
          <Gap sizeRem={1.5} />
          <div className="h-[18.25rem] flex items-center justify-center">
            <Typography size="s" secondary>
              {localizationCaptions[LocalizationKey.NoNewNotifications]}
            </Typography>
          </div>
        </div>
      </ContextMenu>
    </div>
  );
};
