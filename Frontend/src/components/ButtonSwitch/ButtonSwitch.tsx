import React, { FunctionComponent } from 'react';
import { Button } from '../Button/Button';
import { IconNames } from '../../constants';
import { Icon } from '../../pages/Room/components/Icon/Icon';

interface ButtonSwitchProps {
  toggled: boolean;
  onToggle: () => void;
}

export const ButtonSwitch: FunctionComponent<ButtonSwitchProps> = ({
  toggled,
  onToggle,
}) => {
  return (
    <Button
      variant="invertedAlternative"
      className="min-w-unset w-2.375 h-2.375 p-0"
      onClick={onToggle}
    >
      <Icon
        size="s"
        name={toggled ? IconNames.ThemeSwitchLight : IconNames.ThemeSwitchDark}
      />
    </Button>
  );
};
