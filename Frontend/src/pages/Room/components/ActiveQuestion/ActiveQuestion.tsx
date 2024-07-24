import { FunctionComponent, MouseEventHandler, useCallback, useState } from 'react';
import { ActiveQuestionSelector } from '../../../../components/ActiveQuestionSelector/ActiveQuestionSelector';
import { Room, RoomQuestion } from '../../../../types/room';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { ChangeActiveQuestionBody, roomQuestionApiDeclaration, roomsApiDeclaration } from '../../../../apiDeclarations';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

import './ActiveQuestion.css';

export interface ActiveQuestionProps {
  room: Room | null;
  roomQuestions: RoomQuestion[];
  initialQuestion?: RoomQuestion;
  readOnly: boolean;
}

export const ActiveQuestion: FunctionComponent<ActiveQuestionProps> = ({
  room,
  roomQuestions,
  initialQuestion,
  readOnly,
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
    apiMethodState: apiRoomStartReviewMethodState,
    fetchData: fetchRoomStartReview,
  } = useApiMethod<unknown, Room['id']>(roomsApiDeclaration.startReview);
  const {
    process: { loading: loadingRoomStartReview, error: errorRoomStartReview },
  } = apiRoomStartReviewMethodState;

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

  const handleStartReviewRoom = useCallback(() => {
    if (!room?.id) {
      throw new Error('Room id not found');
    }
    fetchRoomStartReview(room.id);
  }, [room?.id, fetchRoomStartReview]);

  const openQuestionsIds = roomQuestions
    .filter(roomQuestion => roomQuestion.state === 'Open')
    .map(roomQuestion => roomQuestion.id);

  return (
    <div className=''>
      <ActiveQuestionSelector
        showClosedQuestions={showClosedQuestions}
        questions={roomQuestions}
        openQuestions={openQuestionsIds}
        initialQuestion={initialQuestion}
        placeHolder={localizationCaptions[LocalizationKey.SelectActiveQuestion]}
        readOnly={readOnly}
        onSelect={handleQuestionSelect}
        onShowClosedQuestions={handleShowClosedQuestions}
        onStartReviewRoom={handleStartReviewRoom}
      />
      {loadingRoomActiveQuestion && <div>{localizationCaptions[LocalizationKey.SendingActiveQuestion]}...</div>}
      {errorRoomActiveQuestion && <div>{localizationCaptions[LocalizationKey.ErrorSendingActiveQuestion]}</div>}
      {loadingRoomStartReview && <div>{localizationCaptions[LocalizationKey.CloseRoomLoading]}...</div>}
      {errorRoomStartReview && <div>{localizationCaptions[LocalizationKey.Error]}: {errorRoomStartReview}</div>}
    </div>
  );
};
