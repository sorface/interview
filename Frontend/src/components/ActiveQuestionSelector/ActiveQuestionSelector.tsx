import React, { ChangeEventHandler, FunctionComponent, MouseEventHandler, useEffect, useRef, useState } from 'react';
import { OpenIcon } from '../OpenIcon/OpenIcon';
import { Localization } from '../../localization';
import { RoomQuestion } from '../../types/room';

import './ActiveQuestionSelector.css';

export interface ActiveQuestionSelectorProps {
  initialQuestionText?: string;
  placeHolder: string;
  showClosedQuestions: boolean;
  questions: RoomQuestion[];
  openQuestions: Array<RoomQuestion['id']>;
  onSelect: (question: RoomQuestion) => void;
  onShowClosedQuestions: MouseEventHandler<HTMLInputElement>;
  onCreate: (value: RoomQuestion['value']) => void;
}

export const ActiveQuestionSelector: FunctionComponent<ActiveQuestionSelectorProps> = ({
  initialQuestionText,
  placeHolder,
  showClosedQuestions,
  questions,
  openQuestions,
  onSelect,
  onShowClosedQuestions,
  onCreate,
}) => {
  const [showMenu, setShowMenu] = useState(false);
  const [selectedValue, setSelectedValue] = useState<RoomQuestion | null>(null);
  const [searchValue, setSearchValue] = useState("");
  const searchRef = useRef<HTMLInputElement>(null);
  const inputRef = useRef<HTMLDivElement>(null);

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
    setShowMenu(!showMenu);
  };

  const getDisplay = () => {
    if (!selectedValue && !initialQuestionText) {
      return placeHolder;
    }
    return `${Localization.ActiveQuestion}: ${selectedValue?.value || initialQuestionText}`;
  };

  const onItemClick = (option: RoomQuestion) => {
    setSelectedValue(option);
    onSelect(option);
  };

  const handleCreate = () => {
    onCreate(searchValue);
    setSearchValue('');
  };

  const onSearch: ChangeEventHandler<HTMLInputElement> = (e) => {
    setSearchValue(e.target.value);
  };

  return (
    <div className="activeQuestionSelector-container">
      <div ref={inputRef} onClick={handleInputClick} className="activeQuestionSelector-input">
        <div className="activeQuestionSelector-selected-value">{getDisplay()}</div>
        <div className="activeQuestionSelector-tools">
          <div className="activeQuestionSelector-tool">
            <OpenIcon sizeRem={1.5} />
          </div>
        </div>
      </div>
      {showMenu && (
        <div className="activeQuestionSelector-menu">
          <div ref={searchRef} className="activeQuestionSelector-search-panel">
            <span>{Localization.ShowClosedQuestions}</span>
            <input type="checkbox" onClick={onShowClosedQuestions} />
            <div className="search-box" >
              <input onChange={onSearch} value={searchValue} />
              {searchValue && (
                <button onClick={handleCreate}>{Localization.CreateQuestion}</button>
              )}
            </div>
          </div>
          {options.length === 0 && (
            <div className='no-questions'>{Localization.NoQuestionsSelector}</div>
          )}
          {options.map((option) => (
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
  );
};
