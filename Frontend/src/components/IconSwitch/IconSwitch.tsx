import React, { FunctionComponent } from 'react';
import { IconNames } from '../../constants';
import { Icon } from '../../pages/Room/components/Icon/Icon';

interface IconSwitchProps {
  toggled: boolean;
}

export const IconSwitch: FunctionComponent<IconSwitchProps> = ({ toggled }) => {
  return (
    <Icon
      size="m"
      name={toggled ? IconNames.ThemeSwitchLight : IconNames.ThemeSwitchDark}
    />
  );
};
