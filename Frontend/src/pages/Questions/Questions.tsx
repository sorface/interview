import React, { FunctionComponent, useCallback, useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { GetQuestionsParams, categoriesApiDeclaration, questionsApiDeclaration } from '../../apiDeclarations';
import { IconNames, pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { ContextMenu } from '../../components/ContextMenu/ContextMenu';
import { Link } from 'react-router-dom';
import { Icon } from '../Room/components/Icon/Icon';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';
import { Category } from '../../types/category';
import { Typography } from '../../components/Typography/Typography';
import { Gap } from '../../components/Gap/Gap';
import { Loader } from '../../components/Loader/Loader';

import './Questions.css';

const pageSize = 10;
const initialPageNumber = 1;

export const Questions: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const navigate = useNavigate();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValueInput, setSearchValueInput] = useState('');
  const { rootCategory, subCategory } = useParams();

  const { apiMethodState: rootCategoryState, fetchData: fetchRootCategory } = useApiMethod<Category | null, Category['id']>(categoriesApiDeclaration.get);
  const { process: { loading: rootCategoryLoading, error: rootCategoryError }, data: rootCategoryData } = rootCategoryState;

  const { apiMethodState: subCategoryState, fetchData: fetchSubCategory } = useApiMethod<Category, Category['id']>(categoriesApiDeclaration.get);
  const { process: { loading: subCategoryLoading, error: subCategoryError }, data: subCategoryData } = subCategoryState;

  const { apiMethodState: questionsState, fetchData: fetchQuestios } = useApiMethod<Question[], GetQuestionsParams>(questionsApiDeclaration.getPage);
  const { process: { loading, error }, data: questions } = questionsState;

  const { apiMethodState: archiveQuestionsState, fetchData: archiveQuestion } = useApiMethod<Question, Question['id']>(questionsApiDeclaration.archive);
  const { process: { loading: archiveLoading, error: archiveError }, data: archivedQuestion } = archiveQuestionsState;

  const categoriesLoading = rootCategoryLoading || subCategoryLoading;
  const categoriesError = rootCategoryError || subCategoryError;

  useEffect(() => {
    fetchQuestios({
      PageNumber: pageNumber,
      PageSize: pageSize,
      tags: [],
      value: searchValueInput,
      categoryId: subCategory?.replace(':category', '') || '',
    });
  }, [pageNumber, searchValueInput, archivedQuestion, subCategory, fetchQuestios]);

  useEffect(() => {
    if (rootCategory) {
      fetchRootCategory(rootCategory);
    }
    if (subCategory) {
      fetchSubCategory(subCategory);
    }
  }, [rootCategory, subCategory, fetchRootCategory, fetchSubCategory]);

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
              key='ContextMenuItemEdit'
              title={localizationCaptions[LocalizationKey.Edit]}
              onClick={handleEditQuestion(question)}
            />,
            <ContextMenu.Item
              key='ContextMenuItemArchive'
              title={archiveLoading ? localizationCaptions[LocalizationKey.ArchiveLoading] : localizationCaptions[LocalizationKey.Archive]}
              onClick={handlearchiveQuestion(question)}
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
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        <Link
          to={
            pathnames.questionsCreate
              .replace(':rootCategory', rootCategory || '')
              .replace(':subCategory', subCategory || '')
          }
        >
          <Button variant='active' className='h-2.5'>
            <Icon name={IconNames.Add} />
            {localizationCaptions[LocalizationKey.CreateQuestion]}
          </Button>
        </Link>
      </PageHeader>
      <div className='questions-page flex-1 overflow-auto'>
        <div className='sticky top-0 bg-form z-1'>
          <div className='flex items-center'>
            {categoriesError && (
              <Typography size='m'>
                {localizationCaptions[LocalizationKey.Error]}: {categoriesError}
              </Typography>
            )}
            {categoriesLoading ? (
              <Loader />
            ) : (
              <>
                <Typography size='m'>
                  {rootCategoryData?.name}
                </Typography>
                <span className='flex opacity-0.65'>
                  <Icon name={IconNames.ChevronForward} />
                </span>
                <span className='opacity-0.5'>
                  <Typography size='m'>
                    {subCategoryData?.name}
                  </Typography>
                </span>
              </>
            )}
          </div>
          <Gap sizeRem={1} />
        </div>
        <ItemsGrid
          currentData={questions}
          loading={loading}
          error={error || archiveError}
          triggerResetAccumData={`${searchValueInput}${subCategory}${archivedQuestion}`}
          loaderClassName='question-item field-wrap'
          renderItem={createQuestionItem}
          nextPageAvailable={questions?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </div>
    </>
  );
};
