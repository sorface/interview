import React, { ChangeEventHandler, FunctionComponent, useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { GetCategoriesParams, categoriesApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { IconNames, pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { ActionModal } from '../../components/ActionModal/ActionModal';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { Category } from '../../types/category';
import { Icon } from '../Room/components/Icon/Icon';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';

import './Categories.css';

const pageSize = 30;
const initialPageNumber = 1;

export const Categories: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValueInput, setSearchValueInput] = useState('');
  const [showOnlyWithoutParent, setShowOnlyWithoutParent] = useState(false);
  const [categoryParent, setCategoryParent] = useState('');

  const { apiMethodState: categoriesState, fetchData: fetchCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading, error }, data: categories } = categoriesState;

  const { apiMethodState: rootCategoriesState, fetchData: fetchRootCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: rootCategoriesLoading, error: rootCategoriesError }, data: rootCategories } = rootCategoriesState;

  const { apiMethodState: archiveCategoryState, fetchData: archiveCategory } = useApiMethod<Category, Category['id']>(categoriesApiDeclaration.archive);
  const { process: { loading: archiveLoading, error: archiveError }, data: archivedCategory } = archiveCategoryState;

  const triggerResetAccumData = `${searchValueInput}${showOnlyWithoutParent}${archivedCategory}${categoryParent}`;

  useEffect(() => {
    fetchCategories({
      PageNumber: pageNumber,
      PageSize: pageSize,
      name: searchValueInput,
      showOnlyWithoutParent,
      ...(categoryParent && { parentId: categoryParent }),
    });
  }, [pageNumber, searchValueInput, archivedCategory, showOnlyWithoutParent, categoryParent, fetchCategories]);

  useEffect(() => {
    fetchRootCategories({
      PageNumber: 1,
      PageSize: pageSize,
      name: '',
      showOnlyWithoutParent: true,
    });
  }, [archivedCategory, fetchRootCategories]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handleOnlyWithoutParentChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    setShowOnlyWithoutParent(e.target.checked);
  };

  const handleCategoryParentChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setCategoryParent(e.target.value);
  };

  const createCategoryItem = useCallback((category: Category) => (
    <li key={category.id}>
      <Field className="category-item">
        <span>{category.name}</span>
        {!category.parentId && <Icon name={IconNames.Clipboard} />}
        <div className="category-controls">
          <Link to={pathnames.categoriesEdit.replace(':id', category.id)}>
            <Button>
              🖊️
            </Button>
          </Link>
          <ActionModal
            openButtonCaption='📁'
            error={archiveError}
            loading={archiveLoading}
            title={localizationCaptions[LocalizationKey.Archive]}
            loadingCaption={localizationCaptions[LocalizationKey.ArchiveLoading]}
            onAction={() => { archiveCategory(category.id) }}
          />
        </div>
      </Field>
    </li>
  ), [archiveLoading, archiveError, localizationCaptions, archiveCategory]);

  return (
    <MainContentWrapper className="categories-page">
      <PageHeader
        title={localizationCaptions[LocalizationKey.CategoriesPageName]}
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        <Link to={pathnames.categoriesCreate}>
          <Button variant='active' className='h-2.5'>
            <Icon name={IconNames.Add} />
            {localizationCaptions[LocalizationKey.CreateCategory]}
          </Button>
        </Link>
      </PageHeader>
      <Field className='!mt-0'>
        <div className="flex">
          <input
            id="showOnlyWithoutParent"
            type="checkbox"
            checked={showOnlyWithoutParent}
            onChange={handleOnlyWithoutParentChange}
          />
          <label htmlFor="showOnlyWithoutParent" className='mr-1'>{localizationCaptions[LocalizationKey.RootCategories]}</label>
          <label htmlFor="parentID">{localizationCaptions[LocalizationKey.Category]}:</label>
          <select id="parentID" value={categoryParent} onChange={handleCategoryParentChange}>
            <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
            {rootCategories?.map(rootCategory => (
              <option key={rootCategory.id} value={rootCategory.id}>{rootCategory.name}</option>
            ))}
          </select>
        </div>
      </Field>
      <ProcessWrapper
        loading={false}
        error={error || archiveError || rootCategoriesError}
      >
        <ItemsGrid
          currentData={categories}
          loading={loading || rootCategoriesLoading}
          error={error || archiveError || rootCategoriesError}
          triggerResetAccumData={triggerResetAccumData}
          loaderClassName='category-item field-wrap'
          renderItem={createCategoryItem}
          nextPageAvailable={categories?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
