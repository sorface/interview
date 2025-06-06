import React from 'react';
import { IconNames } from '../../../../constants';
import { Icon } from '../Icon/Icon';

interface ParticipantPinButtonProps {
  pin: boolean;
  handlePin: () => void;
}

export const ParticipantPinButton = ({
  pin,
  handlePin,
}: ParticipantPinButtonProps) => {
  return (
    <span className="cursor-pointer participant-reaction" onClick={handlePin}>
      <Icon secondary={!pin} name={IconNames.Pin} />
    </span>
  );
};
