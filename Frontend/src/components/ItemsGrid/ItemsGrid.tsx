import React, { useEffect, useState } from 'react';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { useAccumData } from '../../hooks/useAccumData';
import { Loader } from '../../components/Loader/Loader';
import { InfinitePaginator } from '../../components/InfinitePaginator/InfinitePaginator';
import { Typography } from '../Typography/Typography';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Gap } from '../Gap/Gap';

import './ItemsGrid.css';

interface ItemsGridProps<T extends object> {
  currentData: T[] | null;
  loading: boolean;
  error: string | null;
  nextPageAvailable: boolean;
  triggerResetAccumData: string;
  loaderClassName?: string;
  renderItem: (room: T) => JSX.Element;
  handleNextPage: () => void;
}

export const ItemsGrid = <T extends object>({
  currentData,
  loading,
  error,
  nextPageAvailable,
  triggerResetAccumData,
  loaderClassName,
  renderItem,
  handleNextPage,
}: ItemsGridProps<T>) => {
  const localizationCaptions = useLocalizationCaptions();
  const loaders = Array.from({ length: 3 }, () => ({ height: '4rem' }));
  const { accumData, resetAccumData } = useAccumData<T>(currentData);
  const dataLoaded = !loading && currentData;
  const [dataDisplayed, setDataDisplayed] = useState(false);

  useEffect(() => {
    if (!dataLoaded || dataDisplayed) {
      return;
    }
    setDataDisplayed(true);
  }, [dataLoaded, dataDisplayed]);

  useEffect(() => {
    resetAccumData();
  }, [triggerResetAccumData, resetAccumData])

  const noRecords = dataDisplayed && !loading && !accumData.length;

  return (
    <>
      {noRecords && (
        <div className="items-grid-no-data text-grey3">
          <Gap sizeRem={7.25} />
          <Typography size='m'>
            {localizationCaptions[LocalizationKey.NoRecords]}
          </Typography>
        </div>
      )}
      <ul className="items-grid m-0">
        {!!error && (
          <Typography size='m' error>
            <div className='flex items-center'>
              <Icon name={IconNames.Information} />
              <Gap sizeRem={0.25} horizontal />
              <div>
                {localizationCaptions[LocalizationKey.Error]}: {error}
              </div>
            </div>
          </Typography>
        )}
        {!noRecords && (
          accumData.map(renderItem)
        )}
        {(loading || !dataDisplayed) && loaders.map((loader, index) => (
          <div key={`loader${index}`} className={loaderClassName}>
            <div
              style={{ height: loader.height || '1.8rem' }}
            >
              <Loader />
            </div>
          </div>
        ))}
        {!noRecords && nextPageAvailable && (
          <InfinitePaginator onNextPage={handleNextPage} />
        )}
      </ul>
    </>
  );
};
