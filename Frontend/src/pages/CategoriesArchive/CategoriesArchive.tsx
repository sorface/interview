import React, { FunctionComponent, useEffect, useState } from 'react';
import {
  categoriesApiDeclaration,
  PaginationUrlParams,
} from '../../apiDeclarations';
import { useApiMethod } from '../../hooks/useApiMethod';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Typography } from '../../components/Typography/Typography';
import { Gap } from '../../components/Gap/Gap';
import { Category } from '../../types/category';
import { Icon } from '../Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { ActionModal } from '../../components/ActionModal/ActionModal';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';

const pageSize = 10;
const initialPageNumber = 1;

export const CategoriesArchive: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);

  const { apiMethodState: categoriesState, fetchData: fetchCategories } =
    useApiMethod<Category[], PaginationUrlParams>(
      categoriesApiDeclaration.getPageArchived,
    );
  const {
    process: { loading, error },
    data: categories,
  } = categoriesState;

  const {
    apiMethodState: unarchiveCategoryState,
    fetchData: unarchiveCategory,
  } = useApiMethod<Category, Category['id']>(
    categoriesApiDeclaration.unarchive,
  );
  const {
    process: { loading: unarchiveLoading, error: unarchiveError },
    data: unarchivedCategory,
  } = unarchiveCategoryState;

  const triggerResetAccumData = `${unarchivedCategory}`;

  useEffect(() => {
    fetchCategories({
      PageNumber: pageNumber,
      PageSize: pageSize,
    });
  }, [pageNumber, unarchivedCategory, fetchCategories]);

  const handleNextPage = () => {
    setPageNumber(pageNumber + 1);
  };

  const handleUnarchiveCategory = (category: Category) => {
    console.log('handleUnarchiveCategory');
    unarchiveCategory(category.id);
  };

  const createCategoryItem = (category: Category) => (
    <li key={category.id}>
      <Field className="flex items-center bg-wrap">
        <span>{category.name}</span>
        {!category.parentId && <Icon name={IconNames.Clipboard} />}
        <div className="ml-auto">
          <ActionModal
            openButtonCaption={localizationCaptions[LocalizationKey.Unarchive]}
            error={unarchiveError}
            loading={unarchiveLoading}
            title={localizationCaptions[LocalizationKey.Unarchive]}
            loadingCaption={
              localizationCaptions[LocalizationKey.UnarchiveLoading]
            }
            onAction={() => {
              handleUnarchiveCategory(category);
            }}
          />
        </div>
      </Field>
    </li>
  );

  return (
    <MainContentWrapper>
      <PageHeader
        title={localizationCaptions[LocalizationKey.CategoriesPageName]}
      />
      <div className="flex-1 overflow-auto">
        <div className="sticky top-0 bg-form z-1">
          <div className="flex items-center">
            <Typography size="m" bold>
              {localizationCaptions[LocalizationKey.CategoriesArchive]}
            </Typography>
          </div>
          <Gap sizeRem={1} />
        </div>
        <ItemsGrid
          currentData={categories}
          loading={loading}
          error={error || unarchiveError}
          triggerResetAccumData={triggerResetAccumData}
          loaderClassName="field-wrap"
          renderItem={createCategoryItem}
          nextPageAvailable={categories?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </div>
    </MainContentWrapper>
  );
};
