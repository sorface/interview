import { FunctionComponent, MouseEventHandler, useCallback, useState } from 'react';
import { ActiveQuestionSelector } from '../../../../components/ActiveQuestionSelector/ActiveQuestionSelector';
import { Room, RoomQuestion } from '../../../../types/room';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { Question, QuestionType } from '../../../../types/question';
import { ChangeActiveQuestionBody, CreateRoomQuestionBody, roomQuestionApiDeclaration } from '../../../../apiDeclarations';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

import './ActiveQuestion.css';

export interface ActiveQuestionProps {
  room: Room | null;
  roomQuestions: RoomQuestion[];
  initialQuestionText?: string;
}

export const ActiveQuestion: FunctionComponent<ActiveQuestionProps> = ({
  room,
  roomQuestions,
  initialQuestionText,
}) => {
  const [showClosedQuestions, setShowClosedQuestions] = useState(false);
  const localizationCaptions = useLocalizationCaptions();

  const {
    apiMethodState: apiSendActiveQuestionState,
    fetchData: sendRoomActiveQuestion,
  } = useApiMethod<unknown, ChangeActiveQuestionBody>(roomQuestionApiDeclaration.changeActiveQuestion);
  const {
    process: { loading: loadingRoomActiveQuestion, error: errorRoomActiveQuestion },
  } = apiSendActiveQuestionState;

  const {
    apiMethodState: apiCreateQuestionState,
    fetchData: createRoomQuestion,
  } = useApiMethod<unknown, CreateRoomQuestionBody>(roomQuestionApiDeclaration.createQuestion);
  const {
    process: { loading: loadingCreateQuestion, error: errorCreateQuestion },
  } = apiCreateQuestionState;

  const handleShowClosedQuestions: MouseEventHandler<HTMLInputElement> = useCallback((e) => {
    setShowClosedQuestions(e.currentTarget.checked);
  }, []);

  const handleQuestionSelect = useCallback((question: RoomQuestion) => {
    if (!room) {
      throw new Error('Error sending reaction. Room not found.');
    }
    sendRoomActiveQuestion({
      roomId: room.id,
      questionId: question.id,
    });
  }, [room, sendRoomActiveQuestion]);

  const handleQuestionCreate = useCallback((question: Question['value']) => {
    if (!room) {
      throw new Error('Error sending reaction. Room not found.');
    }
    createRoomQuestion({
      roomId: room.id,
      question: {
        value: question,
        tags: [],
        type: QuestionType.Private,
        categoryId: '',
      },
    });
  }, [room, createRoomQuestion]);

  const openQuestionsIds = roomQuestions
    .filter(roomQuestion => roomQuestion.state === 'Open')
    .map(roomQuestion => roomQuestion.id);

  return (
    <div className='active-question-container'>
      <ActiveQuestionSelector
        showClosedQuestions={showClosedQuestions}
        questions={roomQuestions}
        openQuestions={openQuestionsIds}
        initialQuestionText={initialQuestionText}
        placeHolder={localizationCaptions[LocalizationKey.SelectActiveQuestion]}
        onSelect={handleQuestionSelect}
        onShowClosedQuestions={handleShowClosedQuestions}
        onCreate={handleQuestionCreate}
      />
      {loadingRoomActiveQuestion && <div>{localizationCaptions[LocalizationKey.SendingActiveQuestion]}...</div>}
      {errorRoomActiveQuestion && <div>{localizationCaptions[LocalizationKey.ErrorSendingActiveQuestion]}...</div>}
      {loadingCreateQuestion && <div>{localizationCaptions[LocalizationKey.CreatingRoomQuestion]}...</div>}
      {errorCreateQuestion && <div>{localizationCaptions[LocalizationKey.ErrorCreatingRoomQuestion]}...</div>}
    </div>
  );
};
