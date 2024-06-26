import { FunctionComponent, useEffect } from 'react';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Category } from '../../types/category';
import { GetCategoriesParams, categoriesApiDeclaration } from '../../apiDeclarations';
import { Loader } from '../Loader/Loader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

import './CategoriesList.css';

const pageSize = 30;
const initialPageNumber = 1;

interface CategoriesListProps {
  parentId?: GetCategoriesParams['parentId'];
  showOnlyWithoutParent?: boolean;
  onCategoryClick: (category: Category) => void;
}

export const CategoriesList: FunctionComponent<CategoriesListProps> = ({
  parentId,
  showOnlyWithoutParent,
  onCategoryClick,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const { apiMethodState: questionsState, fetchData } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading, error }, data } = questionsState;

  useEffect(() => {
    fetchData({
      PageNumber: initialPageNumber,
      PageSize: pageSize,
      name: '',
      parentId,
      showOnlyWithoutParent,
    });
  }, [fetchData, parentId, showOnlyWithoutParent]);

  const createCategoryItem = (category: Category) => {
    return (
      <div
        key={category.id}
        role='link'
        className='categories-list-item'
        onClick={() => onCategoryClick(category)}
      >
        {category.name}
      </div>
    )
  };

  return (
    <div className='categories-list'>
      {loading && <Loader />}
      {error && <span>{localizationCaptions[LocalizationKey.Error]}: {error}</span>}
      {data?.map(createCategoryItem)}
    </div>
  );
};
