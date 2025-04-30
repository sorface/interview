import React, {
  ChangeEvent,
  FormEvent,
  Fragment,
  FunctionComponent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import {
  CreateCategoryBody,
  GetCategoriesParams,
  UpdateCategoryBody,
  categoriesApiDeclaration,
} from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { Loader } from '../../components/Loader/Loader';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { SubmitField } from '../../components/SubmitField/SubmitField';
import { pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Category } from '../../types/category';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { Gap } from '../../components/Gap/Gap';
import { Checkbox } from '../../components/Checkbox/Checkbox';
import { Typography } from '../../components/Typography/Typography';

import './CategoriesCreate.css';

const pageSize = 30;
const initialPageNumber = 1;
const nameFieldName = 'categoryName';
const orderFieldName = 'categoryOrder';

export const CategoriesCreate: FunctionComponent<{ edit: boolean }> = ({
  edit,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const { apiMethodState: categoryState, fetchData: fetchCreateCategory } =
    useApiMethod<Category['id'], CreateCategoryBody>(
      categoriesApiDeclaration.create,
    );
  const {
    process: { loading, error },
    data: createdCategoryId,
  } = categoryState;

  const {
    apiMethodState: updatingCategoryState,
    fetchData: fetchUpdateCategory,
  } = useApiMethod<Category, UpdateCategoryBody>(
    categoriesApiDeclaration.update,
  );
  const {
    process: { loading: updatingLoading, error: updatingError },
    data: updatedCategoryId,
  } = updatingCategoryState;

  const { apiMethodState: getCategoryState, fetchData: fetchCategory } =
    useApiMethod<Category, Category['id']>(categoriesApiDeclaration.get);
  const {
    process: { loading: categoryLoading, error: categoryError },
    data: category,
  } = getCategoryState;

  const {
    apiMethodState: parentCategoriesState,
    fetchData: fetchParentCategories,
  } = useApiMethod<Category[], GetCategoriesParams>(
    categoriesApiDeclaration.getPage,
  );
  const {
    process: { loading: parentCategoriesLoading, error: parentCategoriesError },
    data: parentCategories,
  } = parentCategoriesState;

  const navigate = useNavigate();
  const { id } = useParams();
  const [categoryName, setCategoryName] = useState('');
  const [categoryParent, setCategoryParent] = useState('');
  const [categoryOrder, setCategoryOrder] = useState(0);
  const [pageNumber, setPageNumber] = useState(initialPageNumber);

  const totalLoading = loading || updatingLoading || categoryLoading;
  const totalError = error || categoryError || updatingError;

  useEffect(() => {
    if (!edit) {
      return;
    }
    if (!id) {
      throw new Error('Category id not found');
    }
    fetchCategory(id);
  }, [edit, id, fetchCategory]);

  useEffect(() => {
    if (!category) {
      return;
    }
    setCategoryName(category.name);
    setCategoryParent(category.parentId);
    setCategoryOrder(category.order);
  }, [category]);

  useEffect(() => {
    if (!createdCategoryId) {
      return;
    }
    toast.success(
      localizationCaptions[LocalizationKey.CategoryCreatedSuccessfully],
    );
    navigate(pathnames.categories);
  }, [createdCategoryId, localizationCaptions, navigate]);

  useEffect(() => {
    if (!updatedCategoryId) {
      return;
    }
    toast.success(
      localizationCaptions[LocalizationKey.CategoryUpdatedSuccessfully],
    );
    navigate(pathnames.categories);
  }, [updatedCategoryId, localizationCaptions, navigate]);

  useEffect(() => {
    fetchParentCategories({
      name: '',
      PageNumber: pageNumber,
      PageSize: pageSize,
      parentId: null,
    });
  }, [pageNumber, fetchParentCategories]);

  const handleNextPage = () => {
    setPageNumber(pageNumber + 1);
  };

  const handleCategoryNameChange = (event: ChangeEvent<HTMLInputElement>) => {
    setCategoryName(event.target.value);
  };

  const handleCategoryOrderChange = (event: ChangeEvent<HTMLInputElement>) => {
    setCategoryOrder(+event.target.value);
  };

  const handleSubmitCreate = useCallback(
    async (event: FormEvent<HTMLFormElement>) => {
      event.preventDefault();

      fetchCreateCategory({
        name: categoryName,
        parentId: categoryParent || null,
        order: categoryOrder,
      });
    },
    [categoryName, categoryParent, categoryOrder, fetchCreateCategory],
  );

  const handleSubmitEdit = useCallback(
    async (event: FormEvent<HTMLFormElement>) => {
      event.preventDefault();
      if (!category) {
        return;
      }
      fetchUpdateCategory({
        id: category.id,
        name: categoryName,
        parentId: categoryParent,
        order: categoryOrder,
      });
    },
    [
      category,
      categoryName,
      categoryParent,
      categoryOrder,
      fetchUpdateCategory,
    ],
  );

  const renderStatus = () => {
    if (totalError) {
      return (
        <Field>
          <div>
            {localizationCaptions[LocalizationKey.Error]}: {totalError}
          </div>
        </Field>
      );
    }
    if (totalLoading) {
      return (
        <Field>
          <Loader />
        </Field>
      );
    }
    return <></>;
  };

  const createQuestionItem = (category: Category) => (
    <Fragment key={category.id}>
      <div className="flex items-center">
        <div>
          <Gap sizeRem={1.25} />
          <Checkbox
            id={category.id}
            checked={categoryParent === category.id}
            onChange={() => setCategoryParent(category.id)}
            label={category.name}
          />
        </div>
      </div>
      <Gap sizeRem={0.25} />
    </Fragment>
  );

  return (
    <MainContentWrapper className="categories-create">
      <HeaderWithLink
        title={localizationCaptions[LocalizationKey.CreateCategory]}
        linkVisible={true}
        path={pathnames.categories}
        linkCaption="<"
        linkFloat="left"
      />
      {renderStatus()}
      <form onSubmit={edit ? handleSubmitEdit : handleSubmitCreate}>
        <Field>
          <div>
            <label htmlFor="name">
              {localizationCaptions[LocalizationKey.CategoryName]}:
            </label>
          </div>
          <input
            id="name"
            name={nameFieldName}
            type="text"
            value={categoryName}
            onChange={handleCategoryNameChange}
          />
        </Field>
        <Field>
          <Typography size="m">
            {localizationCaptions[LocalizationKey.CategoryParent]}:
          </Typography>
          <div className="overflow-auto h-[25rem]">
            <ItemsGrid
              currentData={parentCategories}
              loading={parentCategoriesLoading}
              error={parentCategoriesError}
              triggerResetAccumData=""
              renderItem={createQuestionItem}
              nextPageAvailable={parentCategories?.length === pageSize}
              handleNextPage={handleNextPage}
            />
          </div>
        </Field>
        <Field>
          <div>
            <label htmlFor="order">
              {localizationCaptions[LocalizationKey.CategoryOrder]}:
            </label>
          </div>
          <input
            id="order"
            name={orderFieldName}
            type="number"
            value={categoryOrder}
            className="bg-page-bg"
            onChange={handleCategoryOrderChange}
          />
        </Field>
        <SubmitField caption={localizationCaptions[LocalizationKey.Create]} />
      </form>
    </MainContentWrapper>
  );
};
