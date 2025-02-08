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
    <div onClick={handlePin}>
      {pin && <Icon name={IconNames.ChevronDown} />}
      {!pin && <Icon name={IconNames.Like} />}
    </div>
  );
};
