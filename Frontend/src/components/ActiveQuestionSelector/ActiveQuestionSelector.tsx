import React, { Fragment, FunctionComponent, MouseEventHandler, useEffect, useState } from 'react';
import { LocalizationKey } from '../../localization';
import { RoomQuestion } from '../../types/room';
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

import './ActiveQuestionSelector.css';

const sortOption = (option1: RoomQuestion, option2: RoomQuestion) =>
  option1.order - option2.order;

export interface ActiveQuestionSelectorProps {
  initialQuestion?: RoomQuestion;
  loading: boolean;
  questionsDictionary: Question[];
  questions: RoomQuestion[];
  openQuestions: Array<RoomQuestion['id']>;
  readOnly: boolean;
  onSelect: (question: RoomQuestion) => void;
}

export const ActiveQuestionSelector: FunctionComponent<ActiveQuestionSelectorProps> = ({
  initialQuestion,
  loading,
  questionsDictionary,
  questions,
  openQuestions,
  readOnly,
  onSelect,
}) => {
  const [showMenu, setShowMenu] = useState(false);
  const [selectedValue, setSelectedValue] = useState<RoomQuestion | null>(null);
  const localizationCaptions = useLocalizationCaptions();
  const [questionsCount, setQuestionsCount] = useState(0);
  const [closedQuestionsCount, setClosedQuestionsCount] = useState(0);
  const currentQuestion = initialQuestion || selectedValue;
  const currentQuestionInDictionary =
    currentQuestion &&
    questionsDictionary.find(q => q.id === currentQuestion.id);
  const currentOrder = currentQuestion?.order || 0;
  const [answersModalOpen, setAnswersModalOpen] = useState(false);

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
    if (readOnly) {
      return;
    }
    setShowMenu(!showMenu);
  };

  const getDisplay = () => {
    if (!selectedValue && !initialQuestion) {
      return '';
    }
    return `${currentOrder + 1}. ${selectedValue?.value || initialQuestion?.value}`;
  };

  const onItemClick = (option: RoomQuestion) => {
    setSelectedValue(option);
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
            <div className='ml-auto border border-button border-solid px-0.75 py-0.125 rounded-2'>
              <Typography size='s'>
                {`${closedQuestionsCount} ${localizationCaptions[LocalizationKey.Of]} ${questionsCount}`}
              </Typography>
            </div>
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
              {questions.sort(sortOption).map((question) => (
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
