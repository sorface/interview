import { FunctionComponent, useCallback, useEffect, useState } from 'react';
import { ActiveQuestionSelector } from '../../../../components/ActiveQuestionSelector/ActiveQuestionSelector';
import { Room, RoomQuestion, RoomQuestionEvaluation as RoomQuestionEvaluationType } from '../../../../types/room';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { ChangeActiveQuestionBody, GetRoomQuestionEvaluationParams, MergeRoomQuestionEvaluationBody, roomQuestionApiDeclaration, roomQuestionEvaluationApiDeclaration, roomsApiDeclaration } from '../../../../apiDeclarations';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Gap } from '../../../../components/Gap/Gap';
import { RoomQuestionEvaluation, RoomQuestionEvaluationValue } from '../RoomQuestionEvaluation/RoomQuestionEvaluation';
import { Loader } from '../../../../components/Loader/Loader';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { Button } from '../../../../components/Button/Button';

import './RoomQuestionPanel.css';

const mergeRoomQuestionEvaluationDebounceMs = 1000;
const notFoundCode = 404;

export interface RoomQuestionPanelProps {
  room: Room | null;
  roomQuestionsLoading: boolean;
  roomQuestions: RoomQuestion[];
  initialQuestion?: RoomQuestion;
  readOnly: boolean;
}

export const RoomQuestionPanel: FunctionComponent<RoomQuestionPanelProps> = ({
  room,
  roomQuestionsLoading,
  roomQuestions,
  initialQuestion,
  readOnly,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [roomQuestionEvaluation, setRoomQuestionEvaluation] = useState<RoomQuestionEvaluationValue | null>(null);

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

  const {
    apiMethodState: apiRoomQuestionEvaluationState,
    fetchData: getRoomQuestionEvaluation,
  } = useApiMethod<RoomQuestionEvaluationType, GetRoomQuestionEvaluationParams>(roomQuestionEvaluationApiDeclaration.get);
  const {
    data: loadedRoomQuestionEvaluation,
    process: {
      loading: loadingRoomQuestionEvaluation,
      error: errorRoomQuestionEvaluation,
      code: responseCodeRoomQuestionEvaluation,
    },
  } = apiRoomQuestionEvaluationState;

  const {
    apiMethodState: apiMergeRoomQuestionEvaluationState,
    fetchData: mergeRoomQuestionEvaluation,
  } = useApiMethod<RoomQuestionEvaluationType, MergeRoomQuestionEvaluationBody>(roomQuestionEvaluationApiDeclaration.merge);
  const {
    data: mergedRoomQuestionEvaluation,
    process: {
      loading: loadingMergeRoomQuestionEvaluation,
      error: errorMergeRoomQuestionEvaluation,
    },
  } = apiMergeRoomQuestionEvaluationState;

  const getRoomQuestionEvaluationError = responseCodeRoomQuestionEvaluation !== notFoundCode ? errorRoomQuestionEvaluation : null;
  const totalLoadingRoomQuestionEvaluation = loadingRoomQuestionEvaluation || loadingMergeRoomQuestionEvaluation;
  const totalErrorRoomQuestionEvaluation = errorMergeRoomQuestionEvaluation || getRoomQuestionEvaluationError;

  const openQuestionsIds = roomQuestions
    .filter(roomQuestion => roomQuestion.state === 'Open')
    .map(roomQuestion => roomQuestion.id);

  useEffect(() => {
    if (readOnly || !room || !initialQuestion) {
      return;
    }
    getRoomQuestionEvaluation({
      questionId: initialQuestion.id,
      roomId: room.id,
    });
  }, [readOnly, room, initialQuestion, getRoomQuestionEvaluation]);

  useEffect(() => {
    if (!roomQuestionEvaluation) {
      return;
    }
    const activeQuestion = roomQuestions?.find(question => question.state === 'Active');
    const roomId = room?.id;
    if (!activeQuestion || !roomId) {
      return;
    }
    const requestTimeout = setTimeout(() => {
      mergeRoomQuestionEvaluation({
        ...roomQuestionEvaluation,
        questionId: activeQuestion.id,
        roomId: roomId,
        review: roomQuestionEvaluation.review || '',
        mark: roomQuestionEvaluation.mark || null,
      });
    }, mergeRoomQuestionEvaluationDebounceMs);

    return () => {
      clearTimeout(requestTimeout);
    };
  }, [roomQuestionEvaluation, room?.id, roomQuestions, mergeRoomQuestionEvaluation]);

  useEffect(() => {
    if (!loadedRoomQuestionEvaluation) {
      return;
    }
    setRoomQuestionEvaluation(loadedRoomQuestionEvaluation);
  }, [loadedRoomQuestionEvaluation]);

  useEffect(() => {
    if (responseCodeRoomQuestionEvaluation !== notFoundCode) {
      return;
    }
    setRoomQuestionEvaluation({
      mark: null,
      review: '',
    });
  }, [responseCodeRoomQuestionEvaluation])

  const handleQuestionSelect = useCallback((question: RoomQuestion) => {
    if (!room) {
      throw new Error('Error sending reaction. Room not found.');
    }
    sendRoomActiveQuestion({
      roomId: room.id,
      questionId: question.id,
    });
  }, [room, sendRoomActiveQuestion]);

  const handleNextQuestion = () => {
    if (!room) {
      throw new Error('Error sending reaction. Room not found.');
    }
    sendRoomActiveQuestion({
      roomId: room.id,
      questionId: openQuestionsIds[0],
    });
  };

  const handleStartReviewRoom = useCallback(() => {
    if (!room?.id) {
      throw new Error('Room id not found');
    }
    fetchRoomStartReview(room.id);
  }, [room?.id, fetchRoomStartReview]);

  const handleRoomQuestionEvaluationChange = (newValue: RoomQuestionEvaluationValue) => {
    const activeQuestion = roomQuestions?.find(question => question.state === 'Active');
    const roomId = room?.id;
    if (!activeQuestion || !roomId) {
      return;
    }
    setRoomQuestionEvaluation(newValue);
  };

  return (
    <div className='videochat-field !w-21 flex flex-col'>
      <div className='flex-1 py-1.5 px-1.25 bg-wrap rounded-1.125'>
        <div className='flex flex-col h-full'>
          <ActiveQuestionSelector
            loading={roomQuestionsLoading}
            questionsDictionary={room?.questions || []}
            questions={roomQuestions}
            openQuestions={openQuestionsIds}
            initialQuestion={initialQuestion}
            readOnly={readOnly}
            onSelect={handleQuestionSelect}
          />
        </div>
      </div>
      {!readOnly && (
        <>
          <Gap sizeRem={0.375} />
          <div className='py-1.5 px-1.25 bg-wrap rounded-1.125'>
            {(!loadingRoomQuestionEvaluation && roomQuestionEvaluation) ? (
              <RoomQuestionEvaluation
                value={roomQuestionEvaluation}
                onChange={handleRoomQuestionEvaluationChange}
              />
            ) : (
              !!initialQuestion && (<Loader />)
            )}
            <Gap sizeRem={1} />
            <div className='text-left h-1.125'>
              {mergedRoomQuestionEvaluation && (
                <Typography size='s'>
                  <Icon name={IconNames.CheckmarkDone} />
                  {localizationCaptions[LocalizationKey.Saved]}
                </Typography>
              )}
              {totalLoadingRoomQuestionEvaluation && (<Loader />)}
              {totalErrorRoomQuestionEvaluation && (
                <div>{localizationCaptions[LocalizationKey.Error]}: {totalErrorRoomQuestionEvaluation}</div>
              )}
              {errorRoomActiveQuestion && <div>{localizationCaptions[LocalizationKey.ErrorSendingActiveQuestion]}</div>}
              {loadingRoomStartReview && <div>{localizationCaptions[LocalizationKey.CloseRoomLoading]}...</div>}
              {errorRoomStartReview && <div>{localizationCaptions[LocalizationKey.Error]}: {errorRoomStartReview}</div>}
            </div>
            <Gap sizeRem={1.8125} />
            <Button
              className='w-full flex items-center'
              variant='active'
              onClick={openQuestionsIds.length !== 0 ? handleNextQuestion : handleStartReviewRoom}
            >
              {roomQuestionsLoading || loadingRoomActiveQuestion || loadingRoomStartReview ? (
                <Loader />
              ) : (
                <>
                  <span>
                    {!initialQuestion ?
                      localizationCaptions[LocalizationKey.StartRoom] :
                      localizationCaptions[
                      openQuestionsIds.length !== 0 ?
                        LocalizationKey.NextRoomQuestion :
                        LocalizationKey.StartReviewRoom
                      ]
                    }
                  </span>
                  <Gap sizeRem={0.5} horizontal />
                  <Icon name={openQuestionsIds.length !== 0 ? IconNames.ChevronForward : IconNames.Stop} />
                </>
              )}
            </Button>
          </div>
        </>
      )}
    </div>
  );
};
