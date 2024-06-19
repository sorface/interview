import React, { useEffect, useState } from 'react';
import { Field } from '../../components/FieldsBlock/Field';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { useAccumData } from '../../hooks/useAccumData';
import { Loader } from '../../components/Loader/Loader';
import { InfinitePaginator } from '../../components/InfinitePaginator/InfinitePaginator';

import './ItemsGrid.css';

interface ItemsGridProps<T extends object> {
  currentData: T[] | null;
  loading: boolean;
  nextPageAvailable: boolean;
  triggerResetAccumData: string;
  loaderClassName?: string;
  renderItem: (room: T) => JSX.Element;
  handleNextPage: () => void;
}

export const ItemsGrid = <T extends object>({
  currentData,
  loading,
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

  const noRecords = dataDisplayed && !accumData.length;

  return (
    <>
      <ul className="items-grid">
        {noRecords ? (
          <Field>
            <div className="items-grid-no-data">{localizationCaptions[LocalizationKey.NoRecords]}</div>
          </Field>
        ) : (
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
