import React, { FormEvent, FunctionComponent, useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { CreateRoomBody, CreateTagBody, GetTagsParams, roomsApiDeclaration, tagsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { Loader } from '../../components/Loader/Loader';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { QuestionsSelector } from '../../components/QuestionsSelector/QuestionsSelector';
import { SubmitField } from '../../components/SubmitField/SubmitField';
import { UsersSelector } from '../../components/UsersSelector/UsersSelector';
import { pathnames, toastSuccessOptions } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { User } from '../../types/user';
import { TagsSelector } from '../../components/TagsSelector/TagsSelector';
import { Tag } from '../../types/tag';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { RoomAccessType } from '../../types/room';

import './RoomCreate.css';

const nameFieldName = 'roomName';
const pageNumber = 1;
const pageSize = 30;

export const RoomCreate: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const navigate = useNavigate();
  const { apiMethodState, fetchData } = useApiMethod<string, CreateRoomBody>(roomsApiDeclaration.create);
  const { process: { loading, error }, data: createdRoomId } = apiMethodState;

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

  const [selectedQuestions, setSelectedQuestions] = useState<Question[]>([]);
  const [selectedExperts, setSelectedExperts] = useState<User[]>([]);
  const [selectedExaminees, setSelectedExaminees] = useState<User[]>([]);
  const [tagsSearchValue, setTagsSearchValue] = useState('');
  const [selectedTags, setSelectedTags] = useState<Tag[]>([]);

  const totalLoading = loading || createTagLoading;
  const totalError = error || tagsError || createTagError;

  useEffect(() => {
    fetchTags({
      PageNumber: pageNumber,
      PageSize: pageSize,
      value: tagsSearchValue,
    });
  }, [createdQuestionTag, tagsSearchValue, fetchTags]);

  useEffect(() => {
    if (!createdRoomId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.RoomCreated], toastSuccessOptions);
    navigate(pathnames.rooms);
  }, [createdRoomId, localizationCaptions, navigate]);

  const handleTagSelect = (tag: Tag) => {
    setSelectedTags([...selectedTags, tag]);
  };

  const handleTagUnselect = (tag: Tag) => {
    const newSelectedTags = selectedTags.filter(tg => tg.id !== tag.id);
    setSelectedTags(newSelectedTags);
  };

  const handleTagSearch = (value: string) => {
    setTagsSearchValue(value);
  };

  const handleTagCreate = (tag: Omit<Tag, 'id'>) => {
    fetchCreateTag(tag);
  };

  const handleSubmit = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.target as HTMLFormElement;
    const data = new FormData(form);
    const roomName = data.get(nameFieldName);
    if (!roomName) {
      return;
    }
    if (typeof roomName !== 'string') {
      throw new Error('qestionText field type error');
    }
    fetchData({
      name: roomName,
      questions: selectedQuestions.map((question, index) => ({ id: question.id, order: index })),
      experts: selectedExperts.map(user => user.id),
      examinees: selectedExaminees.map(user => user.id),
      tags: selectedTags.map(tag => tag.id),
      accessType: RoomAccessType.Private,
    });
  }, [selectedQuestions, selectedExperts, selectedExaminees, selectedTags, fetchData]);

  const handleQuestionSelect = useCallback((question: Question) => {
    setSelectedQuestions([...selectedQuestions, question]);
  }, [selectedQuestions]);

  const handleQuestionUnSelect = useCallback((question: Question) => {
    const newSelectedQuestions = selectedQuestions.filter(
      ques => ques.id !== question.id
    );
    setSelectedQuestions(newSelectedQuestions);
  }, [selectedQuestions]);

  const handleExpertSelect = useCallback((user: User) => {
    setSelectedExperts([...selectedExperts, user]);
  }, [selectedExperts]);

  const handleExpertUnSelect = useCallback((user: User) => {
    const newSelectedUsers = selectedExperts.filter(
      usr => usr.id !== user.id
    );
    setSelectedExperts(newSelectedUsers);
  }, [selectedExperts]);

  const handleExamineeSelect = useCallback((user: User) => {
    setSelectedExaminees([...selectedExaminees, user]);
  }, [selectedExaminees]);

  const handleExamineeUnSelect = useCallback((user: User) => {
    const newSelectedUsers = selectedExaminees.filter(
      usr => usr.id !== user.id
    );
    setSelectedExaminees(newSelectedUsers);
  }, [selectedExaminees]);

  const renderStatus = useCallback(() => {
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
  }, [totalError, totalLoading, localizationCaptions]);

  return (
    <MainContentWrapper className="question-create">
      <HeaderWithLink
        title={localizationCaptions[LocalizationKey.CreateRoom]}
        linkVisible={true}
        path={pathnames.rooms}
        linkCaption="<"
        linkFloat="left"
      />
      {renderStatus()}
      <form action="" onSubmit={handleSubmit}>
        <Field>
          <label htmlFor="roomName">{localizationCaptions[LocalizationKey.RoomName]}:</label>
          <input id="roomName" name={nameFieldName} type="text" required />
        </Field>
        <Field>
          <TagsSelector
            placeHolder={localizationCaptions[LocalizationKey.TagsPlaceholder]}
            loading={tagsLoading}
            tags={tags || []}
            selectedTags={selectedTags}
            onSelect={handleTagSelect}
            onUnselect={handleTagUnselect}
            onSearch={handleTagSearch}
            onCreate={handleTagCreate}
          />
        </Field>
        <Field>
          <Link to={pathnames.questions}>{localizationCaptions[LocalizationKey.QuestionsPageName]}:</Link>
          <div className="items-selected">
            {selectedQuestions.map(question => question.value).join(', ')}
          </div>
          <QuestionsSelector
            selected={selectedQuestions}
            onSelect={handleQuestionSelect}
            onUnselect={handleQuestionUnSelect}
          />
        </Field>
        <Field>
          <span>{localizationCaptions[LocalizationKey.RoomExperts]}: </span>
          <span className="items-selected">
            {selectedExperts.map(user => user.nickname).join(', ')}
          </span>
          <UsersSelector
            uniqueKey='Experts'
            selected={selectedExperts}
            onSelect={handleExpertSelect}
            onUnselect={handleExpertUnSelect}
          />
        </Field>
        <Field>
          <span>{localizationCaptions[LocalizationKey.RoomExaminees]}: </span>
          <span className="items-selected">
            {selectedExaminees.map(user => user.nickname).join(', ')}
          </span>
          <UsersSelector
            uniqueKey='Examinees'
            selected={selectedExaminees}
            onSelect={handleExamineeSelect}
            onUnselect={handleExamineeUnSelect}
          />
        </Field>
        <SubmitField caption={localizationCaptions[LocalizationKey.Create]} />
      </form>
    </MainContentWrapper>
  );
};
