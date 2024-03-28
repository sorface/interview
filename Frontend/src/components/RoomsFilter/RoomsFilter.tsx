import { ChangeEventHandler, FunctionComponent } from 'react';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './RoomsFilter.css';

interface RoomsFilterProps {
  participating: boolean;
  closed: boolean;
  onParticipatingChange: (value: boolean) => void;
  onClosedChange: (value: boolean) => void;
}

export const RoomsFilter: FunctionComponent<RoomsFilterProps> = ({
  participating,
  closed,
  onParticipatingChange,
  onClosedChange,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const handleParticipatingChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onParticipatingChange(e.target.checked);
  };

  const handleClosedChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onClosedChange(e.target.checked);
  };

  return (
    <div className="rooms-filter">
      <input
        id="participating-rooms"
        type="checkbox"
        checked={participating}
        onChange={handleParticipatingChange}
      />
      <label htmlFor="participating-rooms">{localizationCaptions[LocalizationKey.ParticipatingRooms]}</label>
      <input
        id="closed-rooms"
        type="checkbox"
        checked={closed}
        onChange={handleClosedChange}
      />
      <label htmlFor="closed-rooms">{localizationCaptions[LocalizationKey.ClosedRooms]}</label>
    </div>
  );
};
