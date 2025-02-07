import { Button } from '../../../../components/Button/Button';
import { IconNames } from '../../../../constants';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { Icon } from '../Icon/Icon';

interface ParticipantPinButtonProps {
  pin: boolean;
  handlePin: () => void;
}

export const ParticipantPinButton = ({
  pin,
  handlePin,
}: ParticipantPinButtonProps) => {
  const localizationCaptions = useLocalizationCaptions();

  return (
    <Button onClick={handlePin}>
      {localizationCaptions[LocalizationKey.Pin]}
      {pin && <Icon name={IconNames.Like} />}
    </Button>
  );
};
