import React, { FunctionComponent, useCallback, useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { GetQuestionsParams, questionsApiDeclaration } from '../../apiDeclarations';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { IconNames, pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { ContextMenu } from '../../components/ContextMenu/ContextMenu';
import { Link } from 'react-router-dom';
import { ThemedIcon } from '../Room/components/ThemedIcon/ThemedIcon';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';

import './Questions.css';

const pageSize = 10;
const initialPageNumber = 1;

export const Questions: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const navigate = useNavigate();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValueInput, setSearchValueInput] = useState('');
  const { category } = useParams();

  const { apiMethodState: questionsState, fetchData: fetchQuestios } = useApiMethod<Question[], GetQuestionsParams>(questionsApiDeclaration.getPage);
  const { process: { loading, error }, data: questions } = questionsState;

  const { apiMethodState: archiveQuestionsState, fetchData: archiveQuestion } = useApiMethod<Question, Question['id']>(questionsApiDeclaration.archive);
  const { process: { loading: archiveLoading, error: archiveError }, data: archivedQuestion } = archiveQuestionsState;

  useEffect(() => {
    fetchQuestios({
      PageNumber: pageNumber,
      PageSize: pageSize,
      tags: [],
      value: searchValueInput,
      categoryId: category?.replace(':category', '') || '',
    });
  }, [pageNumber, searchValueInput, archivedQuestion, category, fetchQuestios]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handleEditQuestion = (question: Question) => () => {
    navigate(pathnames.questionsEdit.replace(':id', question.id));
  };

  const handlearchiveQuestion = (question: Question) => () => {
    archiveQuestion(question.id);
  };

  const createQuestionItem = (question: Question) => (
    <li key={question.id} className='pb-0.25'>
      <QuestionItem
        question={question}
        primary
        contextMenu={{
          position: 'right',
          useButton: true,
          children: [
            <ContextMenu.Item
              title={localizationCaptions[LocalizationKey.Edit]}
              onClick={handleEditQuestion(question)}
            />,
            <ContextMenu.Item
              title={archiveLoading ? localizationCaptions[LocalizationKey.ArchiveLoading] : localizationCaptions[LocalizationKey.Archive]}
              onClick={handlearchiveQuestion(question)}
            />,
          ],
        }}
      />
    </li>
  );

  return (
    <MainContentWrapper className="questions-page">
      <PageHeader
        title={localizationCaptions[LocalizationKey.QuestionsPageName]}
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        <Link to={pathnames.questionsCreate}>
        <Button variant='active' className='h-2.5'>
          <ThemedIcon name={IconNames.Add} />
          {localizationCaptions[LocalizationKey.CreateQuestion]}
        </Button>
        </Link>
      </PageHeader>
      <ProcessWrapper
        loading={false}
        error={error || archiveError}
      >
        <ItemsGrid
          currentData={questions}
          loading={loading}
          triggerResetAccumData={`${searchValueInput}${category}${archivedQuestion}`}
          loaderClassName='question-item field-wrap'
          renderItem={createQuestionItem}
          nextPageAvailable={questions?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
