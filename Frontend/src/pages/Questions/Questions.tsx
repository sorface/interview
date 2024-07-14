import React, { FunctionComponent, useCallback, useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { GetQuestionsParams, questionsApiDeclaration } from '../../apiDeclarations';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { QustionsSearch } from '../../components/QustionsSearch/QustionsSearch';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { ContextMenu } from '../../components/ContextMenu/ContextMenu';

import './Questions.css';

const pageSize = 10;
const initialPageNumber = 1;

export const Questions: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const navigate = useNavigate();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValue, setSearchValue] = useState('');
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
      value: searchValue,
      categoryId: category?.replace(':category', '') || '',
    });
  }, [pageNumber, searchValue, archivedQuestion, category, fetchQuestios]);

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
      <HeaderWithLink
        linkVisible={true}
        path={pathnames.questionsCreate}
        linkCaption="+"
        linkFloat="right"
      >
        <QustionsSearch
          onSearchChange={setSearchValue}
        />
      </HeaderWithLink>
      <ProcessWrapper
        loading={false}
        error={error || archiveError}
      >
        <ItemsGrid
          currentData={questions}
          loading={loading}
          triggerResetAccumData={`${searchValue}${category}${archivedQuestion}`}
          loaderClassName='question-item field-wrap'
          renderItem={createQuestionItem}
          nextPageAvailable={questions?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
