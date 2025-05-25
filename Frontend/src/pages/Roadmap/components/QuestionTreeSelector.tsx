import React, {
  ChangeEventHandler,
  Fragment,
  FunctionComponent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { useApiMethod } from '../../../hooks/useApiMethod';
import { Category } from '../../../types/category';
import {
  GetCategoriesParams,
  GetPageQuestionsTreeResponse,
  GetQuestionsParams,
  GetQuestionsTreesParams,
  PaginationUrlParams,
  categoriesApiDeclaration,
  questionTreeApiDeclaration,
  questionsApiDeclaration,
  roadmapTreeApiDeclaration,
} from '../../../apiDeclarations';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';
import { ModalFooter } from '../../../components/ModalFooter/ModalFooter';
import { Question } from '../../../types/question';
import { ItemsGrid } from '../../../components/ItemsGrid/ItemsGrid';
import { QuestionItem } from '../../../components/QuestionItem/QuestionItem';
import { Gap } from '../../../components/Gap/Gap';
import { RoomQuestionListItem } from '../../../types/room';
import { Button } from '../../../components/Button/Button';
import { Typography } from '../../../components/Typography/Typography';
import { Checkbox } from '../../../components/Checkbox/Checkbox';
import { useDebounce } from '../../../utils/debounce';
import { RoomCreateField } from '../../RoomCreate/RoomCreateField/RoomCreateField';
import { RoomQuestionsSelectorPreview } from '../../RoomCreate/RoomQuestionsSelectorPreview/RoomQuestionsSelectorPreview';
import { Modal } from '../../../components/Modal/Modal';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';
import { Roadmap } from '../../../types/roadmap';
import { TreeMeta } from '../../../types/tree';
import { Field } from '../../../components/FieldsBlock/Field';
import { ProcessWrapper } from '../../../components/ProcessWrapper/ProcessWrapper';
import { QuestionsTree } from '../../../types/questionsTree';

interface QuestionTreeSelectorProps {
  selectedTreeId?: string;
  onSelect: (treeId: string) => void;
}

const pageSize = 10;
const initialPageNumber = 1;

export const QuestionTreeSelector: FunctionComponent<
  QuestionTreeSelectorProps
> = ({ selectedTreeId, onSelect }) => {
  const localizationCaptions = useLocalizationCaptions();
  const [open, setOpen] = useState(false);

  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [searchValueInput, setSearchValueInput] = useState('');
  const searchValueDebounced = useDebounce(searchValueInput);

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

  const { apiMethodState: treeState, fetchData: fetchTree } = useApiMethod<
    QuestionsTree,
    string
  >(questionTreeApiDeclaration.get);
  const {
    process: { loading: treeLoading, error: treeError },
    data: getedTree,
  } = treeState;

  const dataSafe = questionTrees?.data || [];
  const triggerResetAccumData = `${searchValueDebounced}${archivedQuestionTree}`;

  useEffect(() => {
    if (!selectedTreeId) {
      return;
    }
    fetchTree(selectedTreeId);
  }, [selectedTreeId]);

  useEffect(() => {
    if (!open) {
      return;
    }
    fetchQuestionTrees({
      PageNumber: pageNumber,
      PageSize: pageSize,
      name: searchValueDebounced,
    });
  }, [
    pageNumber,
    searchValueDebounced,
    archivedQuestionTree,
    open,
    fetchQuestionTrees,
  ]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handleSelectTree = (tree: TreeMeta) => {
    onSelect(tree.id);
    setOpen(false);
  };

  const createItem = useCallback(
    (tree: TreeMeta) => (
      <li key={tree.id}>
        <div className="flex">
          <div>{tree.name}</div>
          <Button
            variant="active2"
            className="ml-auto min-w-[0rem] w-[2.375rem] h-[2.375rem] !p-[0rem]"
            onClick={() => handleSelectTree(tree)}
          >
            <Icon size="s" name={IconNames.ChevronForward} />
          </Button>
        </div>
        <Gap sizeRem={0.5} />
      </li>
    ),
    [archiveLoading, archiveError, localizationCaptions, archiveQuestionTree],
  );

  return (
    <>
      <div
        className="flex items-center w-[18rem] mr-[0.5rem]"
        onClick={() => setOpen(true)}
      >
        <Icon size="s" name={IconNames.Settings} />
        <Gap sizeRem={0.25} horizontal />
        {getedTree && <Typography size="m">{getedTree.name}</Typography>}
      </div>
      <Modal open={open} onClose={() => setOpen(false)} contentLabel="">
        <div className="flex flex-col">
          <input
            type="text"
            value={searchValueInput}
            onChange={(e) => setSearchValueInput(e.target.value)}
          />
          <Gap sizeRem={1.5} />
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
          <ModalFooter>
            <Button onClick={() => setOpen(false)}>
              {localizationCaptions[LocalizationKey.Cancel]}
            </Button>
          </ModalFooter>
        </div>
      </Modal>
    </>
  );
};
