import React, { FunctionComponent, useEffect, useState } from 'react';
import { PaginationUrlParams, questionsApiDeclaration } from '../../apiDeclarations';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { ContextMenu } from '../../components/ContextMenu/ContextMenu';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Typography } from '../../components/Typography/Typography';
import { Gap } from '../../components/Gap/Gap';

const pageSize = 10;
const initialPageNumber = 1;

export const QuestionsArchive: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  
  const { apiMethodState: questionsState, fetchData: fetchQuestios } = useApiMethod<Question[], PaginationUrlParams>(questionsApiDeclaration.getPageArchived);
  const { process: { loading, error }, data: questions } = questionsState;
  
  const { apiMethodState: unarchiveQuestionsState, fetchData: unarchiveQuestion } = useApiMethod<Question, Question['id']>(questionsApiDeclaration.unarchive);
  const { process: { loading: unarchiveLoading, error: unarchiveError }, data: unarchivedQuestion } = unarchiveQuestionsState;
  
  const triggerResetAccumData = `${unarchivedQuestion}`;

  useEffect(() => {
    fetchQuestios({
      PageNumber: pageNumber,
      PageSize: pageSize,
    });
  }, [pageNumber, unarchivedQuestion, fetchQuestios]);

  const handleNextPage = () => {
    setPageNumber(pageNumber + 1);
  };

  const handleUnarchiveQuestion = (question: Question) => () => {
    unarchiveQuestion(question.id);
  };

  const createQuestionItem = (question: Question) => (
    <li key={question.id} className='pb-0.25'>
      <QuestionItem
        question={question}
        categoryName={question.category.name}
        primary
        contextMenu={{
          position: 'bottom-right',
          useButton: true,
          children: [
            <ContextMenu.Item
              key='ContextMenuItemArchive'
              title={unarchiveLoading ? localizationCaptions[LocalizationKey.UnarchiveLoading] : localizationCaptions[LocalizationKey.Unarchive]}
              onClick={handleUnarchiveQuestion(question)}
            />,
          ],
        }}
      />
    </li>
  );

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.QuestionsPageName]}
      />
      <div className='flex-1 overflow-auto'>
        <div className='sticky top-0 bg-form z-1'>
          <div className='flex items-center'>
            <Typography size='m' bold>
              {localizationCaptions[LocalizationKey.QuestionsArchive]}
            </Typography>
          </div>
          <Gap sizeRem={1} />
        </div>
        <ItemsGrid
          currentData={questions}
          loading={loading}
          error={error || unarchiveError}
          triggerResetAccumData={triggerResetAccumData}
          loaderClassName='field-wrap'
          renderItem={createQuestionItem}
          nextPageAvailable={questions?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </div>
    </>
  );
};
