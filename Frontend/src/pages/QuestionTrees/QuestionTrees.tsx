import React, {
  FunctionComponent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { Link } from 'react-router-dom';
import {
  GetPageQuestionsTreeResponse,
  GetQuestionsTreesParams,
  questionTreeApiDeclaration,
} from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { IconNames, pathnames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { ActionModal } from '../../components/ActionModal/ActionModal';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { Icon } from '../Room/components/Icon/Icon';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';
import { TreeMeta } from '../../types/tree';

const pageSize = 30;
const initialPageNumber = 1;

export const QuestionTrees: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValueInput, setSearchValueInput] = useState('');

  const { apiMethodState: questionTreesState, fetchData: fetchQuestionTrees } =
    useApiMethod<GetPageQuestionsTreeResponse, GetQuestionsTreesParams>(
      questionTreeApiDeclaration.getPage,
    );
  const {
    process: { loading, error },
    data: questionTrees,
  } = questionTreesState;

  const {
    apiMethodState: archiveQuestionTreeState,
    fetchData: archiveQuestionTree,
  } = useApiMethod<unknown, string>(questionTreeApiDeclaration.archive);
  const {
    process: { loading: archiveLoading, error: archiveError },
    data: archivedQuestionTree,
  } = archiveQuestionTreeState;

  const dataSafe = questionTrees?.data || [];
  const triggerResetAccumData = `${searchValueInput}${archivedQuestionTree}`;

  useEffect(() => {
    fetchQuestionTrees({
      PageNumber: pageNumber,
      PageSize: pageSize,
      name: searchValueInput,
    });
  }, [pageNumber, searchValueInput, archivedQuestionTree, fetchQuestionTrees]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const createItem = useCallback(
    (tree: TreeMeta) => (
      <li key={tree.id}>
        <Field className="flex items-center">
          <span>{tree.name}</span>
          <div className="ml-auto">
            <Link to={pathnames.questionTreeEdit.replace(':id', tree.id)}>
              <Button>🖊️</Button>
            </Link>
            <ActionModal
              openButtonCaption="📁"
              error={archiveError}
              loading={archiveLoading}
              title={localizationCaptions[LocalizationKey.Archive]}
              loadingCaption={
                localizationCaptions[LocalizationKey.ArchiveLoading]
              }
              onAction={() => {
                archiveQuestionTree(tree.id);
              }}
            />
          </div>
        </Field>
      </li>
    ),
    [archiveLoading, archiveError, localizationCaptions, archiveQuestionTree],
  );

  return (
    <MainContentWrapper className="">
      <PageHeader
        title={localizationCaptions[LocalizationKey.QuestionTreesPageName]}
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        <Link to={pathnames.questionTreeCreate}>
          <Button variant="active" className="h-[2.5rem]">
            <Icon name={IconNames.Add} />
            {localizationCaptions[LocalizationKey.CreateQuestionTree]}
          </Button>
        </Link>
      </PageHeader>
      <ProcessWrapper loading={false} error={error || archiveError}>
        <ItemsGrid
          currentData={dataSafe}
          loading={loading}
          error={error || archiveError}
          triggerResetAccumData={triggerResetAccumData}
          loaderClassName="field-wrap"
          renderItem={createItem}
          nextPageAvailable={dataSafe.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
