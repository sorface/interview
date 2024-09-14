import { FunctionComponent, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Category } from '../../types/category';
import { GetCategoriesParams, categoriesApiDeclaration } from '../../apiDeclarations';
import { Loader } from '../Loader/Loader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames, pathnames } from '../../constants';

import './CategoriesList.css';

const pageSize = 30;
const initialPageNumber = 1;

interface CategoriesListProps {
  activeId?: Category['id'] | null;
  parentId?: GetCategoriesParams['parentId'];
  showOnlyWithoutParent?: boolean;
  onCategoryClick: (category: Category) => void;
}

export const CategoriesList: FunctionComponent<CategoriesListProps> = ({
  activeId,
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
        className={`categories-list-item ${category.id === activeId ? 'active' : ''}`}
        onClick={() => onCategoryClick(category)}
      >
        <span>{category.name}</span>
        {!parentId && (
          <Icon size='s' name={IconNames.ChevronForward} />
        )}
      </div>
    )
  };

  return (
    <div className='categories-list overflow-x-hidden overflow-y-auto min-h-4'>
      {loading && (
        <div className='categories-list-item h-fit'>
          <Loader />
        </div>
      )}
      {error && <span>{localizationCaptions[LocalizationKey.Error]}: {error}</span>}
      {data?.map(createCategoryItem)}
      {!parentId && (
        <div className='categories-list-item'>
          <Link to={pathnames.questionsArchive}>
            {localizationCaptions[LocalizationKey.QuestionsArchive]}
          </Link>
        </div>
      )}
    </div>
  );
};
