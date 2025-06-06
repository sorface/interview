import React, { ChangeEventHandler, FunctionComponent, useState } from 'react';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Button } from '../Button/Button';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Gap } from '../Gap/Gap';

export interface PageHeaderSearchProps {
  searchValue: string;
  onSearchChange: (value: string) => void;
}

export const PageHeaderSearch: FunctionComponent<PageHeaderSearchProps> = ({
  searchValue,
  onSearchChange,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [open, setOpen] = useState(false);

  const handleOpenSwitch = () => {
    setOpen(!open);
  };

  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    onSearchChange(e.target.value);
  };

  return (
    <div className="flex items-stretch justify-end h-[2.5rem]">
      <input
        type="text"
        className="muted"
        placeholder={localizationCaptions[LocalizationKey.SearchByName]}
        value={searchValue}
        style={{
          display: open ? 'block' : 'none',
        }}
        onChange={handleSearchChange}
      />
      {open && <Gap sizeRem={0.25} horizontal />}
      <Button
        variant="invertedAlternative"
        className="min-w-[0rem] w-[2.5rem] h-[2.5rem] !px-[0rem] !py-[0rem]"
        onClick={handleOpenSwitch}
      >
        <Icon size="s" name={IconNames.Search} />
      </Button>
    </div>
  );
};
