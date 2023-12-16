import React, { FormEvent, FunctionComponent, useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
import { Localization } from '../../localization';

import './RoomCreate.css';

const nameFieldName = 'roomName';
const twitchChannelFieldName = 'roomTwitchChannel';
const pageNumber = 1;
const pageSize = 30;

export const RoomCreate: FunctionComponent = () => {
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
    toast.success(Localization.RoomCreated, toastSuccessOptions);
    navigate(pathnames.rooms);
  }, [createdRoomId, navigate]);

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
    const roomTwitchChannel = data.get(twitchChannelFieldName);
    if (!roomTwitchChannel) {
      return;
    }
    if (typeof roomTwitchChannel !== 'string') {
      throw new Error('roomTwitchChannel field type error');
    }
    fetchData({
      name: roomName,
      twitchChannel: roomTwitchChannel,
      questions: selectedQuestions.map(question => question.id),
      experts: selectedExperts.map(user => user.id),
      examinees: selectedExaminees.map(user => user.id),
      tags: selectedTags.map(tag => tag.id),
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
          <div>{Localization.Error}: {totalError}</div>
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
  }, [totalError, totalLoading]);

  return (
    <MainContentWrapper className="question-create">
      <HeaderWithLink
        title={Localization.CreateRoom}
        linkVisible={true}
        path={pathnames.rooms}
        linkCaption="<"
        linkFloat="left"
      />
      {renderStatus()}
      <form action="" onSubmit={handleSubmit}>
        <Field>
          <label htmlFor="roomName">{Localization.RoomName}:</label>
          <input id="roomName" name={nameFieldName} type="text" required />
        </Field>
        <Field>
          <label htmlFor="twitchChannel">{Localization.RoomTwitchChannel}:</label>
          <input id="twitchChannel" name={twitchChannelFieldName} type="text" required />
        </Field>
        <Field>
          <TagsSelector
            placeHolder={Localization.TagsPlaceholder}
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
          <div>Questions:</div>
          <div>{Localization.RoomQuestions}:</div>
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
          <span>{Localization.RoomExperts}: </span>
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
          <span>{Localization.RoomExaminees}: </span>
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
        <SubmitField caption={Localization.Create} />
      </form>
    </MainContentWrapper>
  );
};
