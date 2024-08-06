import { FunctionComponent } from 'react';
import { IconNames } from '../../../../constants';

interface ThemedIconProps {
  name: IconNames;
  size?: 'small' | 'large';
}

export const Icon: FunctionComponent<ThemedIconProps> = ({
  name,
  size,
}) => {
  return (
    <ion-icon name={`${name}`} size={size}></ion-icon>
  );
};
