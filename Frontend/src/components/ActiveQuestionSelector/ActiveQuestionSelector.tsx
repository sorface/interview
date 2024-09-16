import React, { Fragment, FunctionComponent, MouseEventHandler, useEffect, useState } from 'react';
import { LocalizationKey } from '../../localization';
import { Room, RoomParticipant, RoomQuestion } from '../../types/room';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Gap } from '../Gap/Gap';
import { Typography } from '../Typography/Typography';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Modal } from '../Modal/Modal';
import { ModalFooter } from '../ModalFooter/ModalFooter';
import { Button } from '../Button/Button';
import { QuestionAnswers } from '../QuestionAnswers/QuestionAnswers';
import { Question } from '../../types/question';
import { useParticipantTypeLocalization } from '../../hooks/useParticipantTypeLocalization';
import { useApiMethod } from '../../hooks/useApiMethod';
import { roomsApiDeclaration } from '../../apiDeclarations';

import './ActiveQuestionSelector.css';

const participantsUpdateIntervalMs = 5000;

export interface ActiveQuestionSelectorProps {
  roomId?: string;
  initialQuestion?: RoomQuestion;
  loading: boolean;
  questionsDictionary: Question[];
  questions: RoomQuestion[];
  openQuestions: Array<RoomQuestion['id']>;
  readOnly: boolean;
  onSelect: (question: RoomQuestion) => void;
}

export const ActiveQuestionSelector: FunctionComponent<ActiveQuestionSelectorProps> = ({
  roomId,
  initialQuestion,
  loading,
  questionsDictionary,
  questions,
  openQuestions,
  readOnly,
  onSelect,
}) => {
  const { apiMethodState, fetchData: fetchRoom } = useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const { data: room, process: { error: roomError } } = apiMethodState;

  const localizeParticipantType = useParticipantTypeLocalization();
  const [showMenu, setShowMenu] = useState(false);
  const [roomParticipants, setRoomParticipants] = useState<RoomParticipant[]>([]);
  const localizationCaptions = useLocalizationCaptions();
  const [questionsCount, setQuestionsCount] = useState(0);
  const [closedQuestionsCount, setClosedQuestionsCount] = useState(0);
  const currentQuestionInDictionary =
    initialQuestion &&
    questionsDictionary.find(q => q.id === initialQuestion.id);
  const currentOrder = initialQuestion?.order || 0;
  const [answersModalOpen, setAnswersModalOpen] = useState(false);

  useEffect(() => {
    if (!roomId || initialQuestion) {
      return;
    }
    const updateRoomData = () => {
      fetchRoom(roomId);
    };
    const intervalUpdate = setInterval(updateRoomData, participantsUpdateIntervalMs);
    updateRoomData();

    return () => {
      clearInterval(intervalUpdate);
    };
  }, [roomId, initialQuestion, fetchRoom]);

  useEffect(() => {
    if (!room?.participants) {
      return;
    }
    setRoomParticipants(room.participants);
  }, [room?.participants])

  useEffect(() => {
    if (loading) {
      return;
    }
    setQuestionsCount(questions.length);
    setClosedQuestionsCount(questions.length - openQuestions.length);
  }, [loading, questions.length, openQuestions.length]);

  const isOpened = (question: RoomQuestion) => {
    return openQuestions.includes(question.id);
  }

  const handleInputClick: MouseEventHandler<HTMLDivElement> = () => {
    setShowMenu(!showMenu);
  };

  const getDisplay = () => {
    if (!initialQuestion) {
      return '';
    }
    return `${currentOrder + 1}. ${initialQuestion?.value}`;
  };

  const onItemClick = (option: RoomQuestion) => {
    if (readOnly) {
      return;
    }
    setShowMenu(false);
    onSelect(option);
  };

  const handleAnswersModalOpen = () => {
    setAnswersModalOpen(true);
  };

  const handleAnswersModalClose = () => {
    setAnswersModalOpen(false);
  };

  return (
    <>
      <div className="activeQuestionSelector-container relative">
        <div onClick={handleInputClick} className="activeQuestionSelector-input cursor-pointer">
          <Icon name={showMenu ? IconNames.ChevronBack : IconNames.ReorderFour} />
          <Gap sizeRem={1} horizontal />
          <div className="activeQuestionSelector-selected-value w-full flex items-center">
            <div>
              <Typography size='m'>
                {localizationCaptions[LocalizationKey.RoomQuestions]}
              </Typography>
            </div>
            {!!initialQuestion && (
              <div className='ml-auto border border-button border-solid px-0.75 py-0.125 rounded-2'>
                <Typography size='s'>
                  {`${closedQuestionsCount} ${localizationCaptions[LocalizationKey.Of]} ${questionsCount}`}
                </Typography>
              </div>
            )}
          </div>
        </div>
        <Gap sizeRem={1} />
        <progress className='w-full h-0.125' value={closedQuestionsCount} max={questionsCount}></progress>
        {showMenu && (
          <div className="text-left">
            {questions.length === 0 && (
              <div className='no-questions'>{localizationCaptions[LocalizationKey.NoQuestionsSelector]}</div>
            )}
            <Gap sizeRem={1} />
            <div className='grid grid-cols-questions-list gap-y-0.5'>
              {questions.map((question) => (
                <Fragment
                  key={question.id}
                >
                  <Typography size='m'>{question.order + 1}.</Typography>
                  <div
                    className='cursor-pointer overflow-hidden whitespace-nowrap text-ellipsis'
                    onClick={() => onItemClick(question)}
                  >
                    <Typography size='m'>{question.value}</Typography>
                  </div>
                  <div className='text-dark-green-light'>
                    {!isOpened(question) && (
                      <Icon size='s' name={IconNames.Checkmark} />
                    )}
                  </div>
                </Fragment>
              ))}
            </div>
          </div>
        )}
        {(!initialQuestion && !showMenu) && (
          <div>
            <Gap sizeRem={1} />
            <Typography size='m' bold>
              {localizationCaptions[LocalizationKey.RoomParticipants]}:
            </Typography>
            <Gap sizeRem={0.5} />
            {!!roomError && (
              <Typography size='m' error>
                <div className='flex items-center'>
                  <Icon name={IconNames.Information} size='s' />
                  <Gap sizeRem={0.25} horizontal />
                  {localizationCaptions[LocalizationKey.Error]}: {roomError}
                </div>
              </Typography>
            )}
            {roomParticipants.map(participant => (
              <Fragment key={participant.id}>
                <div className='flex items-baseline'>
                  <Typography size='m'>{participant.nickname}</Typography>
                  <Gap sizeRem={0.5} horizontal />
                  <Typography size='s' secondary>{localizeParticipantType(participant.type)}</Typography>
                </div>
                <Gap sizeRem={0.5} />
              </Fragment>
            ))}
          </div>
        )}
      </div>
      {!showMenu && (
        <>
          <Gap sizeRem={1} />
          <div className='text-left'>
            <Typography size='l' bold>
              {getDisplay()}
            </Typography>
          </div>
        </>
      )}
      {!!(!readOnly && currentQuestionInDictionary?.answers?.length) && (
        <div className='cursor-pointer mt-auto text-right' onClick={handleAnswersModalOpen}>
          <Typography size='s' secondary>{localizationCaptions[LocalizationKey.QuestionAnswerOptions]}</Typography>
        </div>
      )}
      <Modal
        contentLabel={localizationCaptions[LocalizationKey.QuestionAnswerOptions]}
        open={answersModalOpen}
        onClose={handleAnswersModalClose}
      >
        {currentQuestionInDictionary?.answers && (
          <QuestionAnswers
            answers={currentQuestionInDictionary.answers}
            codeEditor={currentQuestionInDictionary.codeEditor}
          />
        )}
        <ModalFooter>
          <Button onClick={handleAnswersModalClose}>
            {localizationCaptions[LocalizationKey.Close]}
          </Button>
        </ModalFooter>
      </Modal>
    </>
  );
};
