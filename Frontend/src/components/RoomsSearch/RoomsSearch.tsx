import { ChangeEventHandler, FunctionComponent } from 'react';
import { Localization } from '../../localization';

import './RoomsSearch.css';

interface RoomsSearchProps {
  searchValue: string;
  onSearchChange: (value: string) => void;
}

export const RoomsSearch: FunctionComponent<RoomsSearchProps> = ({
  searchValue,
  onSearchChange,
}) => {
  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onSearchChange(e.target.value);
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
    </div>
  );
};
