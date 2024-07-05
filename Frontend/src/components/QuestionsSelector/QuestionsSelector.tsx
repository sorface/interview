import { ChangeEvent, FunctionComponent, useCallback, useEffect, useState } from 'react';
import { GetQuestionsParams, questionsApiDeclaration } from '../../apiDeclarations';
import { ProcessWrapper } from '../ProcessWrapper/ProcessWrapper';
import { Paginator } from '../../components/Paginator/Paginator';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { QustionsSearch } from '../QustionsSearch/QustionsSearch';
import { DragNDropListItem } from '../DragNDropList/DragNDropList';

import './QuestionsSelector.css';

const pageSize = 10;
const initialPageNumber = 1;

interface QuestionsSelectorProps {
  selected: DragNDropListItem[];
  onSelect: (question: Question) => void;
  onUnselect: (question: Question) => void;
}

export const QuestionsSelector: FunctionComponent<QuestionsSelectorProps> = ({
  selected,
  onSelect,
  onUnselect,
}) => {
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValue, setSearchValue] = useState('');
  const { apiMethodState, fetchData } = useApiMethod<Question[], GetQuestionsParams>(questionsApiDeclaration.getPage);
  const { process: { loading, error }, data: questions } = apiMethodState;
  const questionsSafe: Question[] = questions || [];

  useEffect(() => {
    fetchData({
      PageNumber: pageNumber,
      PageSize: pageSize,
      value: searchValue,
      tags: [],
      categoryId: '',
    });
  }, [fetchData, pageNumber, searchValue]);

  const handleCheckboxChange = useCallback((event: ChangeEvent<HTMLInputElement>) => {
    const { value, checked } = event.target;
    if (!questions) {
      return;
    }
    const questionItem = questions.find(
      question => question.id === value
    );
    if (!questionItem) {
      throw new Error('Question item not found in state');
    }
    if (checked) {
      onSelect(questionItem);
    } else {
      onUnselect(questionItem);
    }
  }, [questions, onSelect, onUnselect]);

  const createQuestionItem = useCallback((question: Question) => (
    <li key={question.id} className='questions-selector-item'>
      <input
        id={`input-${question.id}`}
        type="checkbox"
        value={question.id}
        checked={selected.some(que => que.id === question.id)}
        onChange={handleCheckboxChange}
      />
      <label htmlFor={`input-${question.id}`}>
        {question.value} [
        {question.tags.map(tag => (
          <span
            key={tag.id}
            className='questions-selector-item-tag'
            style={{ borderColor: `#${tag.hexValue}` }}
          >
            {tag.value}
          </span>
        ))}
        ]
      </label>
    </li>
  ), [selected, handleCheckboxChange]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handlePrevPage = useCallback(() => {
    setPageNumber(pageNumber - 1);
  }, [pageNumber]);

  return (
    <>
      <QustionsSearch
        onSearchChange={setSearchValue}
      />
      <ProcessWrapper
        loading={loading}
        error={error}
        loaders={Array.from({ length: pageSize + 1 }, () => ({ height: '0.25rem' }))}
      >
        <>
          <ul className="questions-selector">
            {questionsSafe.map(createQuestionItem)}
          </ul>
          <Paginator
            pageNumber={pageNumber}
            prevDisabled={pageNumber === initialPageNumber}
            nextDisabled={questionsSafe.length !== pageSize}
            onPrevClick={handlePrevPage}
            onNextClick={handleNextPage}
          />
        </>
      </ProcessWrapper>
    </>
  );
};
