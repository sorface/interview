import { ChangeEventHandler, FunctionComponent } from "react";
import { useLocalizationCaptions } from "../../hooks/useLocalizationCaptions";
import { LocalizationKey } from "../../localization";

export interface PageHeaderSearchProps {
  searchValue: string;
  onSearchChange: (value: string) => void;
}

export const PageHeaderSearch: FunctionComponent<PageHeaderSearchProps> = ({
  searchValue,
  onSearchChange,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onSearchChange(e.target.value);
  };

  return (
    <div className="flex items-stretch h-2.5">
      <input
        type="text"
        placeholder={localizationCaptions[LocalizationKey.SearchByName]}
        value={searchValue}
        onChange={handleSearchChange}
      />
    </div>
  );
};
