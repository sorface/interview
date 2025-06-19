import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useState,
} from 'react';
import { generatePath, Link } from 'react-router-dom';
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
import { Gap } from '../../components/Gap/Gap';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';

const pageSize = 30;
const initialPageNumber = 1;

export const QuestionsRootCategories: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
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
      showOnlyWithoutParent: true,
    });
  }, [pageNumber, searchValueInput, fetchCategories]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const createCategoryItem = useCallback((category: Category) => {
    const path = generatePath(pathnames.questionsSubCategories, {
      rootCategory: category.id,
    });
    return (
      <li key={category.id}>
        <Link to={path} className="no-underline">
          <div className="flex items-center bg-wrap p-[1rem] mb-[1rem] rounded-[0.5rem]">
            <Typography size="l">{category.name}</Typography>
          </div>
        </Link>
      </li>
    );
  }, []);

  return (
    <MainContentWrapper>
      <PageHeader
        title={localizationCaptions[LocalizationKey.QuestionsPageName]}
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        {admin && (
          <Link to={pathnames.categoriesCreate}>
            <Button variant="active" className="h-[2.5rem]">
              <Icon name={IconNames.Add} />
              {localizationCaptions[LocalizationKey.CreateCategory]}
            </Button>
          </Link>
        )}
      </PageHeader>
      {admin && (
        <div className="text-left">
          <Link to={pathnames.questionsArchive}>
            <Typography size="l">
              {localizationCaptions[LocalizationKey.QuestionsArchive]}
            </Typography>
          </Link>
          <Gap sizeRem={1} />
        </div>
      )}
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
