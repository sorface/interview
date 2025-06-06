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
      className="min-w-[0rem] w-[2.375rem] h-[2.375rem] !p-[0rem]"
      onClick={onToggle}
    >
      <Icon
        size="s"
        name={toggled ? IconNames.ThemeSwitchLight : IconNames.ThemeSwitchDark}
      />
    </Button>
  );
};
