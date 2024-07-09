import React, { FunctionComponent, useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { GetQuestionsParams, questionsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { QustionsSearch } from '../../components/QustionsSearch/QustionsSearch';
import { ActionModal } from '../../components/ActionModal/ActionModal';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';

import './Questions.css';

const pageSize = 10;
const initialPageNumber = 1;

export const Questions: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
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

  const createQuestionItem = useCallback((question: Question) => (
    <li key={question.id}>
      <Field className="question-item">
        <span>{question.value}</span>
        <div className="question-controls">
        <Link to={pathnames.questionsEdit.replace(':id', question.id)}>
          <button>
            🖊️
          </button>
        </Link>
        <ActionModal
          openButtonCaption='📁'
          error={archiveError}
          loading={archiveLoading}
          title={localizationCaptions[LocalizationKey.Archive]}
          loadingCaption={localizationCaptions[LocalizationKey.ArchiveLoading]}
          onAction={() => {archiveQuestion(question.id)}}
        />
        </div>
      </Field>
    </li>
  ), [archiveLoading, archiveError, localizationCaptions, archiveQuestion]);

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
        error={error|| archiveError}
      >
        <ItemsGrid
          currentData={questions}
          loading={loading}
          triggerResetAccumData={`${searchValue}${category}`}
          loaderClassName='question-item field-wrap'
          renderItem={createQuestionItem}
          nextPageAvailable={questions?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
