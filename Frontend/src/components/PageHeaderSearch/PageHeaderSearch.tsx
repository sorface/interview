import { ChangeEventHandler, FunctionComponent, useState } from 'react';
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
    <div className='flex items-stretch justify-end h-2.5'>
      <input
        type='text'
        placeholder={localizationCaptions[LocalizationKey.SearchByName]}
        value={searchValue}
        style={{
          opacity: open ? '1' : '0',
          transition: 'opacity 0.1s ease-in-out',
        }}
        onChange={handleSearchChange}
      />
      <Gap sizeRem={0.25} horizontal />
      <Button
        variant='invertedAlternative'
        className='min-w-unset w-2.5 h-2.5 p-0'
        onClick={handleOpenSwitch}
      >
        <Icon size='s' name={IconNames.Search} />
      </Button>
    </div>
  );
};
