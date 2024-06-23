import React, { ChangeEvent, ChangeEventHandler, FormEvent, FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateQuestionBody, GetCategoriesParams, UpdateQuestionBody, categoriesApiDeclaration, questionsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { Loader } from '../../components/Loader/Loader';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { SubmitField } from '../../components/SubmitField/SubmitField';
import { pathnames, toastSuccessOptions } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question, QuestionType } from '../../types/question';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';
import { Category } from '../../types/category';

import './QuestionCreate.css';

const valueFieldName = 'qestionText';

export const QuestionCreate: FunctionComponent<{ edit: boolean; }> = ({ edit }) => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const {
    apiMethodState: questionState,
    fetchData: fetchCreateQuestion,
  } = useApiMethod<Question['id'], CreateQuestionBody>(questionsApiDeclaration.create);
  const { process: { loading, error }, data: createdQuestionId } = questionState;

  const { apiMethodState: updatingQuestionState, fetchData: fetchUpdateQuestion } = useApiMethod<Question, UpdateQuestionBody>(questionsApiDeclaration.update);
  const {
    process: { loading: updatingLoading, error: updatingError },
    data: updatedQuestionId,
  } = updatingQuestionState;

  const {
    apiMethodState: getQuestionState,
    fetchData: fetchQuestion,
  } = useApiMethod<Question, Question['id']>(questionsApiDeclaration.get);
  const { process: { loading: questionLoading, error: questionError }, data: question } = getQuestionState;

  const { apiMethodState: rootCategoriesState, fetchData: fetchRootCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: rootCategoriesLoading, error: rootCategoriesError }, data: rootCategories } = rootCategoriesState;

  const { apiMethodState: subCategoriesState, fetchData: fetchSubCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: subCategoriesLoading, error: subCategoriesError }, data: subCategories } = subCategoriesState;

  const navigate = useNavigate();
  let { id } = useParams();
  const [questionValue, setQuestionValue] = useState('');
  const [type, setType] = useState<QuestionType>(QuestionType.Private);
  const [rootCategory, setRootCategory] = useState('');
  const [subCategory, setSubCategory] = useState('');

  const totalLoading = loading || updatingLoading || questionLoading || rootCategoriesLoading || subCategoriesLoading;
  const totalError = error || questionError || updatingError || rootCategoriesError || subCategoriesError;

  useEffect(() => {
    if (!edit) {
      return;
    }
    if (!id) {
      throw new Error('Question id not found');
    }
    fetchQuestion(id);
  }, [edit, id, fetchQuestion]);

  useEffect(() => {
    fetchRootCategories({
      name: '',
      PageNumber: 1,
      PageSize: 30,
      showOnlyWithoutParent: true,
    });
  }, [fetchRootCategories]);

  useEffect(() => {
    fetchSubCategories({
      name: '',
      PageNumber: 1,
      PageSize: 30,
      parentId: rootCategory,
    });
  }, [rootCategory, fetchSubCategories]);

  useEffect(() => {
    if (!question) {
      return;
    }
    setQuestionValue(question.value);
  }, [question]);

  useEffect(() => {
    if (!createdQuestionId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.QuestionCreatedSuccessfully], toastSuccessOptions);
    navigate(pathnames.questions);
  }, [createdQuestionId, localizationCaptions, navigate]);

  useEffect(() => {
    if (!updatedQuestionId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.QuestionUpdatedSuccessfully], toastSuccessOptions);
    navigate(pathnames.questions);
  }, [updatedQuestionId, localizationCaptions, navigate]);

  const handleQuestionValueChange = (event: ChangeEvent<HTMLInputElement>) => {
    setQuestionValue(event.target.value);
  };

  const handleSubmitCreate = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    fetchCreateQuestion({
      value: questionValue,
      tags: [],
      type,
      categoryId: subCategory,
    });
  }, [questionValue, type, subCategory, fetchCreateQuestion]);

  const handleSubmitEdit = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!question) {
      return;
    }
    fetchUpdateQuestion({
      id: question.id,
      value: questionValue,
      tags: [],
      type,
      categoryId: subCategory,
    });

  }, [question, questionValue, type, subCategory, fetchUpdateQuestion]);

  const handleTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    setType(e.target.value as QuestionType);
  };

  const handleRootCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setRootCategory(e.target.value);
  };

  const handleSubCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setSubCategory(e.target.value);
  };

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
    <MainContentWrapper className="question-create">
      <HeaderWithLink
        title={localizationCaptions[LocalizationKey.CreateQuestion]}
        linkVisible={true}
        path={pathnames.questions}
        linkCaption="<"
        linkFloat="left"
      />
      {renderStatus()}
      <form onSubmit={edit ? handleSubmitEdit : handleSubmitCreate}>
        <Field>
          <select id="rootCategory" value={rootCategory} onChange={handleRootCategoryChange}>
            <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
            {rootCategories?.map(rootCategory => (
              <option key={rootCategory.id} value={rootCategory.id}>{rootCategory.name}</option>
            ))}
          </select>
          <select id="subCategory" value={subCategory} onChange={handleSubCategoryChange}>
            <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
            {subCategories?.map(subCategory => (
              <option key={subCategory.id} value={subCategory.id}>{subCategory.name}</option>
            ))}
          </select>
        </Field>
        <Field>
          <div><label htmlFor="qestionText">{localizationCaptions[LocalizationKey.QuestionText]}:</label></div>
          <input id="qestionText" name={valueFieldName} type="text" value={questionValue} onChange={handleQuestionValueChange} />
        </Field>
        {admin && (
          <Field>
            <div><label htmlFor="qestionType">{localizationCaptions[LocalizationKey.QuestionType]}:</label></div>
            <select id="qestionType" value={type} onChange={handleTypeChange}>
              <option value={QuestionType.Private}>{localizationCaptions[LocalizationKey.QuestionTypePrivate]}</option>
              <option value={QuestionType.Public}>{localizationCaptions[LocalizationKey.QuestionTypePublic]}</option>
            </select>
          </Field>
        )}
        <SubmitField caption={localizationCaptions[LocalizationKey.Create]} />
      </form>
    </MainContentWrapper>
  );
};
