import { FunctionComponent, MouseEventHandler, useCallback, useState } from 'react';
import { ActiveQuestionSelector } from '../../../../components/ActiveQuestionSelector/ActiveQuestionSelector';
import { Room, RoomQuestion } from '../../../../types/room';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { Question } from '../../../../types/question';
import { ChangeActiveQuestionBody, CreateRoomQuestionBody, roomQuestionApiDeclaration } from '../../../../apiDeclarations';
import { Localization } from '../../../../localization';

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
        placeHolder={Localization.SelectActiveQuestion}
        onSelect={handleQuestionSelect}
        onShowClosedQuestions={handleShowClosedQuestions}
        onCreate={handleQuestionCreate}
      />
      {loadingRoomActiveQuestion && <div>{Localization.SendingActiveQuestion}...</div>}
      {errorRoomActiveQuestion && <div>{Localization.ErrorSendingActiveQuestion}...</div>}
      {loadingCreateQuestion && <div>{Localization.CreatingRoomQuestion}...</div>}
      {errorCreateQuestion && <div>{Localization.ErrorCreatingRoomQuestion}...</div>}
    </div>
  );
};
