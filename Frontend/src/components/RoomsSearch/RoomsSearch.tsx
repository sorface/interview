import { ChangeEventHandler, FunctionComponent } from 'react';
import { Localization } from '../../localization';

import './RoomsSearch.css';

interface RoomsSearchProps {
  searchValue: string;
  participating: boolean;
  closed: boolean;
  onSearchChange: (value: string) => void;
  onParticipatingChange: (value: boolean) => void;
  onClosedChange: (value: boolean) => void;
}

export const RoomsSearch: FunctionComponent<RoomsSearchProps> = ({
  searchValue,
  participating,
  closed,
  onSearchChange,
  onParticipatingChange,
  onClosedChange,
}) => {
  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onSearchChange(e.target.value);
  };

  const handleParticipatingChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onParticipatingChange(e.target.checked);
  };

  const handleClosedChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onClosedChange(e.target.checked);
  };

  return (
    <div className="rooms-search">
      <input
        type="text"
        className="qustions-search-value"
        placeholder={Localization.SearchByName}
        value={searchValue}
        onChange={handleSearchChange}
      />
      <input
        id="participating-rooms"
        type="checkbox"
        checked={participating}
        onChange={handleParticipatingChange}
      />
      <label htmlFor="participating-rooms">{Localization.ParticipatingRooms}</label>
      <input
        id="closed-rooms"
        type="checkbox"
        checked={closed}
        onChange={handleClosedChange}
      />
      <label htmlFor="closed-rooms">{Localization.ClosedRooms}</label>
    </div>
  );
};
