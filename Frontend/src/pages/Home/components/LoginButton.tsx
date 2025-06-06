import React, { FunctionComponent } from 'react';
import { Button } from '../../../components/Button/Button';
import { Gap } from '../../../components/Gap/Gap';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';

interface LoginButtonProps {
  caption: string;
  onClick?: () => void;
}

export const LoginButton: FunctionComponent<LoginButtonProps> = ({
  caption,
  onClick,
}) => {
  return (
    <Button variant="invertedActive" onClick={onClick}>
      {caption}
      <Gap horizontal sizeRem={0.15} />
      <Icon name={IconNames.LogIn} />
    </Button>
  );
};
