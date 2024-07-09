import { ChangeEventHandler, FunctionComponent, useEffect, useState } from 'react';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './QustionsSearch.css'

interface QustionsSearchProps {
  onSearchChange: (value: string) => void;
}

export const QustionsSearch: FunctionComponent<QustionsSearchProps> = ({
  onSearchChange,
}) => {
  const [searchValue, setSearchValue] = useState('');
  const localizationCaptions = useLocalizationCaptions();

  useEffect(
    () => onSearchChange(searchValue),
    [searchValue, onSearchChange]
  );

  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    setSearchValue(e.target.value);
  };

  return (
    <div className="qustions-search">
      <input
        type="text"
        className="qustions-search-value"
        placeholder={localizationCaptions[LocalizationKey.SearchByValue]}
        value={searchValue}
        onChange={handleSearchChange}
      />
    </div>
  );
};
