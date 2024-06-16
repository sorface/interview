import React, { useEffect, useState } from 'react';
import { Field } from '../../components/FieldsBlock/Field';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { useAccumData } from '../../hooks/useAccumData';
import { Loader } from '../../components/Loader/Loader';
import { InfinitePaginator } from '../../components/InfinitePaginator/InfinitePaginator';

import './ItemsGrid.css';

interface ItemsGridProps<T extends object> {
  currentData: T[];
  loading: boolean;
  nextPageAvailable: boolean;
  renderItem: (room: T) => JSX.Element;
  handleNextPage: () => void;
}

export const ItemsGrid = <T extends object>({
  currentData,
  loading,
  nextPageAvailable,
  renderItem,
  handleNextPage,
}: ItemsGridProps<T>) => {
  const localizationCaptions = useLocalizationCaptions();
  const loaders = Array.from({ length: 3 }, () => ({ height: '4rem' }));
  const accumData = useAccumData<T>(currentData);
  const dataLoaded = !loading && accumData.length !== 0;
  const [dataDisplayed, setDataDisplayed] = useState(false);

  useEffect(() => {
    if (!dataLoaded || dataDisplayed) {
      return;
    }
    setDataDisplayed(true);
  }, [dataLoaded, dataDisplayed]);

  return (
    <>
      <ul className="items-grid">
        {(!accumData.length && !loading) ? (
          <Field>
            <div className="items-grid-no-data">{localizationCaptions[LocalizationKey.NoRecords]}</div>
          </Field>
        ) : (
          <>
            {accumData.map(renderItem)}
            {nextPageAvailable && (
              <InfinitePaginator onNextPage={handleNextPage} />
            )}
          </>
        )}
        {loading && loaders.map((loader, index) => (
          <Field key={`loader${index}`}>
            <div
              style={{ height: loader.height || '1.8rem' }}
              className="process-wrapper-loader"
            >
              <Loader />
            </div>
          </Field>
        ))}
      </ul>
    </>
  );
};
