import { ChangeEventHandler, FunctionComponent } from 'react';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

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
        placeholder={useLocalizationCaptions()[LocalizationKey.SearchByName]}
        value={searchValue}
        onChange={handleSearchChange}
      />
    </div>
  );
};
