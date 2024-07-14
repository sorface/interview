import { ChangeEventHandler, FunctionComponent } from 'react';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './RoomsFilter.css';

interface RoomsFilterProps {
  searchValue: string;
  onSearchChange: (value: string) => void;
}

export const RoomsFilter: FunctionComponent<RoomsFilterProps> = ({
  searchValue,
  onSearchChange,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onSearchChange(e.target.value);
  };

  return (
    <div className="rooms-filter flex items-center">
      <input
        type="text"
        className="qustions-search-value "
        placeholder={localizationCaptions[LocalizationKey.SearchByName]}
        value={searchValue}
        onChange={handleSearchChange}
      />
    </div>
  );
};
