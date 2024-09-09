import React, { ChangeEventHandler, FunctionComponent, MouseEventHandler, useEffect, useRef, useState } from 'react';
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
  showClosedQuestions: boolean;
  loading: boolean;
  questionsDictionary: Question[];
  questions: RoomQuestion[];
  openQuestions: Array<RoomQuestion['id']>;
  readOnly: boolean;
  onSelect: (question: RoomQuestion) => void;
  onShowClosedQuestions: MouseEventHandler<HTMLInputElement>;
}

export const ActiveQuestionSelector: FunctionComponent<ActiveQuestionSelectorProps> = ({
  initialQuestion,
  showClosedQuestions,
  loading,
  questionsDictionary,
  questions,
  openQuestions,
  readOnly,
  onSelect,
  onShowClosedQuestions,
}) => {
  const [showMenu, setShowMenu] = useState(false);
  const [selectedValue, setSelectedValue] = useState<RoomQuestion | null>(null);
  const [searchValue, setSearchValue] = useState("");
  const searchRef = useRef<HTMLInputElement>(null);
  const inputRef = useRef<HTMLDivElement>(null);
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

  const questionsFiltered = questions.filter(
    question => showClosedQuestions ? !isOpened(question) : isOpened(question)
  );

  const getOptions = () => {
    if (!searchValue) {
      return questionsFiltered;
    }

    return questionsFiltered.filter(
      (question) =>
        question.value.toLowerCase().indexOf(searchValue.toLowerCase()) >= 0
    );
  };

  const options = getOptions();

  useEffect(() => {
    setSearchValue("");
    if (showMenu && searchRef.current) {
      searchRef.current.focus();
    }
  }, [showMenu]);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (!e.target) {
        return;
      }
      const inputRefContainsTarget = inputRef.current?.contains(e.target as any);
      const searchRefTarget = searchRef.current?.contains(e.target as any);
      const shouldClose = !inputRefContainsTarget && !searchRefTarget;
      if (shouldClose) {
        setShowMenu(false);
      }
    };

    window.addEventListener('click', handler);
    return () => {
      window.removeEventListener('click', handler);
    };
  });

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
    onSelect(option);
  };

  const onSearch: ChangeEventHandler<HTMLInputElement> = (e) => {
    setSearchValue(e.target.value);
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
        <div ref={inputRef} onClick={handleInputClick} className="activeQuestionSelector-input cursor-pointer">
          <Icon name={IconNames.ReorderFour} />
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
          <div className="activeQuestionSelector-menu text-left">
            <div ref={searchRef} className="activeQuestionSelector-search-panel">
              <span>{localizationCaptions[LocalizationKey.ShowClosedQuestions]}</span>
              <input type="checkbox" checked={showClosedQuestions} onClick={onShowClosedQuestions} />
              <div className="search-box" >
                <input onChange={onSearch} value={searchValue} />
              </div>
            </div>
            {options.length === 0 && (
              <div className='no-questions'>{localizationCaptions[LocalizationKey.NoQuestionsSelector]}</div>
            )}
            {options.sort(sortOption).map((option) => (
              <div
                onClick={() => onItemClick(option)}
                key={option.value}
                className={`activeQuestionSelector-item ${!isOpened(option) && 'closed'}`}
              >
                {option.value}
              </div>
            ))}
          </div>
        )}
      </div>
      <Gap sizeRem={1} />
      <div className='text-left'>
        <Typography size='l' bold>
          {getDisplay()}
        </Typography>
      </div>
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
