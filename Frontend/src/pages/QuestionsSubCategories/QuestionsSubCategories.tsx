import React, {
  FunctionComponent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { generatePath, Link, useParams } from 'react-router-dom';
import {
  GetCategoriesParams,
  categoriesApiDeclaration,
} from '../../apiDeclarations';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { IconNames, pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { Category } from '../../types/category';
import { Icon } from '../Room/components/Icon/Icon';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';
import { Typography } from '../../components/Typography/Typography';

const pageSize = 30;
const initialPageNumber = 1;

export const QuestionsSubCategories: FunctionComponent = () => {
  const { rootCategory } = useParams();
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValueInput, setSearchValueInput] = useState('');

  const { apiMethodState: categoriesState, fetchData: fetchCategories } =
    useApiMethod<Category[], GetCategoriesParams>(
      categoriesApiDeclaration.getPage,
    );
  const {
    process: { loading, error },
    data: categories,
  } = categoriesState;

  const triggerResetAccumData = `${searchValueInput}`;

  useEffect(() => {
    fetchCategories({
      PageNumber: pageNumber,
      PageSize: pageSize,
      name: searchValueInput,
      parentId: rootCategory,
    });
  }, [pageNumber, searchValueInput, rootCategory, fetchCategories]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const createCategoryItem = useCallback(
    (category: Category) => {
      const path = generatePath(pathnames.questions, {
        rootCategory: rootCategory,
        subCategory: category.id,
      });
      return (
        <li key={category.id}>
          <div className="flex items-center bg-wrap p-[1rem] mb-[1rem] rounded-[0.5rem]">
            <Link to={path}>
              <Typography size="l">{category.name}</Typography>
            </Link>
          </div>
        </li>
      );
    },
    [rootCategory],
  );

  return (
    <MainContentWrapper>
      <PageHeader
        title={localizationCaptions[LocalizationKey.CategoriesPageName]}
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        <Link to={pathnames.categoriesCreate}>
          <Button variant="active" className="h-[2.5rem]">
            <Icon name={IconNames.Add} />
            {localizationCaptions[LocalizationKey.CreateCategory]}
          </Button>
        </Link>
      </PageHeader>
      <ProcessWrapper loading={false} error={error}>
        <ItemsGrid
          currentData={categories}
          loading={loading}
          error={error}
          triggerResetAccumData={triggerResetAccumData}
          loaderClassName="field-wrap"
          renderItem={createCategoryItem}
          nextPageAvailable={categories?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
