import React, { ChangeEvent, FormEvent, FunctionComponent, useCallback, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateCategoryBody, GetCategoriesParams, UpdateCategoryBody, categoriesApiDeclaration } from '../../apiDeclarations';
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

import './CategoriesCreate.css';

const nameFieldName = 'categoryName';

export const CategoriesCreate: FunctionComponent<{ edit: boolean; }> = ({ edit }) => {
  const localizationCaptions = useLocalizationCaptions();
  const {
    apiMethodState: categoryState,
    fetchData: fetchCreateCategory,
  } = useApiMethod<Category['id'], CreateCategoryBody>(categoriesApiDeclaration.create);
  const { process: { loading, error }, data: createdCategoryId } = categoryState;

  const { apiMethodState: updatingCategoryState, fetchData: fetchUpdateCategory } = useApiMethod<Category, UpdateCategoryBody>(categoriesApiDeclaration.update);
  const {
    process: { loading: updatingLoading, error: updatingError },
    data: updatedCategoryId,
  } = updatingCategoryState;

  const {
    apiMethodState: getCategoryState,
    fetchData: fetchCategory,
  } = useApiMethod<Category, Category['id']>(categoriesApiDeclaration.get);
  const { process: { loading: categoryLoading, error: categoryError }, data: category } = getCategoryState;

  const { apiMethodState: parentCategoriesState, fetchData: fetchParentCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: parentCategoriesLoading, error: parentCategoriesError }, data: parentCategories } = parentCategoriesState;

  const navigate = useNavigate();
  let { id } = useParams();
  const [categoryName, setCategoryName] = useState('');
  const [categoryParent, setCategoryParent] = useState('');

  const totalLoading = loading || updatingLoading || categoryLoading || parentCategoriesLoading;
  const totalError = error || categoryError || updatingError || parentCategoriesError;

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
  }, [category]);

  useEffect(() => {
    if (!createdCategoryId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.CategoryCreatedSuccessfully]);
    navigate(pathnames.categories);
  }, [createdCategoryId, localizationCaptions, navigate]);

  useEffect(() => {
    if (!updatedCategoryId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.CategoryUpdatedSuccessfully]);
    navigate(pathnames.categories);
  }, [updatedCategoryId, localizationCaptions, navigate]);

  useEffect(() => {
    fetchParentCategories({
      name: '',
      PageNumber: 1,
      PageSize: 30,
      parentId: null,
    });
  }, [fetchParentCategories]);

  const handleCategoryNameChange = (event: ChangeEvent<HTMLInputElement>) => {
    setCategoryName(event.target.value);
  };

  const handleCategoryParentChange = (e: ChangeEvent<HTMLSelectElement>) => {
    setCategoryParent(e.target.value);
  };

  const handleSubmitCreate = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    fetchCreateCategory({
      name: categoryName,
      parentId: categoryParent || null,
    });

  }, [categoryName, categoryParent, fetchCreateCategory]);

  const handleSubmitEdit = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!category) {
      return;
    }
    fetchUpdateCategory({
      id: category.id,
      name: categoryName,
      parentId: categoryParent,
    });

  }, [category, categoryName, categoryParent, fetchUpdateCategory]);

  const renderStatus = () => {
    if (totalError) {
      return (
        <Field>
          <div>{localizationCaptions[LocalizationKey.Error]}: {totalError}</div>
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
          <div><label htmlFor="name">{localizationCaptions[LocalizationKey.CategoryName]}:</label></div>
          <input id="name" name={nameFieldName} type="text" value={categoryName} onChange={handleCategoryNameChange} />
        </Field>
        <Field>
          <div><label htmlFor="parentID">{localizationCaptions[LocalizationKey.CategoryParent]}:</label></div>
            <select id="parentID" value={categoryParent} onChange={handleCategoryParentChange}>
            <option value='null'>{localizationCaptions[LocalizationKey.NotSelected]}</option>
              {parentCategories?.map(parentCategory => (
                <option key={parentCategory.id} value={parentCategory.id}>{parentCategory.name}</option>
              ))}
            </select>
        </Field>
        <SubmitField caption={localizationCaptions[LocalizationKey.Create]} />
      </form>
    </MainContentWrapper>
  );
};
