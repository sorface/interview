import React, { ChangeEvent, FormEvent, FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateQuestionBody, CreateTagBody, GetTagsParams, UpdateQuestionBody, questionsApiDeclaration, tagsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { Loader } from '../../components/Loader/Loader';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { SubmitField } from '../../components/SubmitField/SubmitField';
import { pathnames, toastSuccessOptions } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question, QuestionType } from '../../types/question';
import { Tag } from '../../types/tag';
import { TagsSelector } from '../../components/TagsSelector/TagsSelector';
import { LocalizationKey } from '../../localization';
import { HeaderField } from '../../components/HeaderField/HeaderField';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';

import './QuestionCreate.css';

const valueFieldName = 'qestionText';
const pageNumber = 1;
const pageSize = 30;

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

  const {
    apiMethodState: tagsState,
    fetchData: fetchTags,
  } = useApiMethod<Tag[], GetTagsParams>(tagsApiDeclaration.getPage);
  const { process: { loading: tagsLoading, error: tagsError }, data: tags } = tagsState;

  const {
    apiMethodState: tagCreateState,
    fetchData: fetchCreateTag,
  } = useApiMethod<Tag, CreateTagBody>(tagsApiDeclaration.createTag);
  const { process: { loading: createTagLoading, error: createTagError }, data: createdQuestionTag } = tagCreateState;

  const navigate = useNavigate();
  let { id } = useParams();
  const [questionValue, setQuestionValue] = useState('');
  const [tagsSearchValue, setTagsSearchValue] = useState('');
  const [selectedTags, setSelectedTags] = useState<Tag[]>([]);
  const [type, setType] = useState<QuestionType>(QuestionType.Private);

  const totalLoading = loading || createTagLoading || updatingLoading || questionLoading;
  const totalError = error || questionError || tagsError || updatingError || createTagError;

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
    if (!question) {
      return;
    }
    setQuestionValue(question.value);
    setSelectedTags(question.tags);
  }, [question]);

  useEffect(() => {
    fetchTags({
      PageNumber: pageNumber,
      PageSize: pageSize,
      value: tagsSearchValue,
    });
  }, [createdQuestionTag, tagsSearchValue, fetchTags]);

  useEffect(() => {
    if (!createdQuestionTag) {
      return;
    }
    fetchTags({
      PageNumber: pageNumber,
      PageSize: pageSize,
      value: '',
    });
  }, [createdQuestionTag, fetchTags]);

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

  const handleSelect = (tag: Tag) => {
    setSelectedTags([...selectedTags, tag]);
  };

  const handleUnselect = (tag: Tag) => {
    const newSelectedTags = selectedTags.filter(tg => tg.id !== tag.id);
    setSelectedTags(newSelectedTags);
  };

  const handleTagSearch = (value: string) => {
    setTagsSearchValue(value);
  };

  const handleTagCreate = (tag: Omit<Tag, 'id'>) => {
    fetchCreateTag(tag);
  };

  const handleQuestionValueChange = (event: ChangeEvent<HTMLInputElement>) => {
    setQuestionValue(event.target.value);
  };

  const handleSubmitCreate = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    fetchCreateQuestion({
      value: questionValue,
      tags: selectedTags.map(tag => tag.id),
      type,
    });

  }, [selectedTags, questionValue, type, fetchCreateQuestion]);

  const handleSubmitEdit = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!question) {
      return;
    }
    fetchUpdateQuestion({
      id: question.id,
      value: questionValue,
      tags: selectedTags.map(tag => tag.id),
      type,
    });

  }, [selectedTags, question, questionValue, type, fetchUpdateQuestion]);

  const handleTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    setType(e.target.value as QuestionType);
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
      <HeaderField />
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
        <Field>
          <TagsSelector
            placeHolder={localizationCaptions[LocalizationKey.TagsPlaceholder]}
            loading={tagsLoading}
            tags={tags || []}
            selectedTags={selectedTags}
            onSelect={handleSelect}
            onUnselect={handleUnselect}
            onSearch={handleTagSearch}
            onCreate={handleTagCreate}
          />
        </Field>
        <SubmitField caption={localizationCaptions[LocalizationKey.Create]} />
      </form>
    </MainContentWrapper>
  );
};
