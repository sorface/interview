import { ChangeEventHandler, FunctionComponent } from 'react';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './RoomsFilter.css';

interface RoomsFilterProps {
  participating: boolean;
  closed: boolean;
  searchValue: string;
  onSearchChange: (value: string) => void;
  onParticipatingChange: (value: boolean) => void;
  onClosedChange: (value: boolean) => void;
}

export const RoomsFilter: FunctionComponent<RoomsFilterProps> = ({
  participating,
  closed,
  searchValue,
  onSearchChange,
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

  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onSearchChange(e.target.value);
  };

  return (
    <div className="rooms-filter flex items-center">
      <input
        id="participating-rooms"
        type="checkbox"
        checked={participating}
        onChange={handleParticipatingChange}
      />
      <label htmlFor="participating-rooms" className='pr-0.5'>{localizationCaptions[LocalizationKey.ParticipatingRooms]}</label>
      <input
        id="closed-rooms"
        type="checkbox"
        checked={closed}
        onChange={handleClosedChange}
      />
      <label htmlFor="closed-rooms" className='pr-0.5'>{localizationCaptions[LocalizationKey.ClosedRooms]}</label>
      <input
        type="text"
        className="qustions-search-value "
        placeholder={useLocalizationCaptions()[LocalizationKey.SearchByName]}
        value={searchValue}
        onChange={handleSearchChange}
      />
    </div>
  );
};
