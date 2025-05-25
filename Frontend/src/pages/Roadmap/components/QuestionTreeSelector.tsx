import React, {
  FunctionComponent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { useApiMethod } from '../../../hooks/useApiMethod';
import {
  GetPageQuestionsTreeResponse,
  GetQuestionsTreesParams,
  questionTreeApiDeclaration,
} from '../../../apiDeclarations';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';
import { ModalFooter } from '../../../components/ModalFooter/ModalFooter';
import { ItemsGrid } from '../../../components/ItemsGrid/ItemsGrid';
import { Gap } from '../../../components/Gap/Gap';
import { Button } from '../../../components/Button/Button';
import { Typography } from '../../../components/Typography/Typography';
import { useDebounce } from '../../../utils/debounce';
import { Modal } from '../../../components/Modal/Modal';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';
import { TreeMeta } from '../../../types/tree';
import { QuestionsTree } from '../../../types/questionsTree';
import { Loader } from '../../../components/Loader/Loader';

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

  const { apiMethodState: treeState, fetchData: fetchTree } = useApiMethod<
    QuestionsTree,
    string
  >(questionTreeApiDeclaration.get);
  const {
    process: { loading: treeLoading, error: treeError },
    data: getedTree,
  } = treeState;

  const dataSafe = questionTrees?.data || [];
  const triggerResetAccumData = `${searchValueDebounced}`;

  useEffect(() => {
    if (!selectedTreeId) {
      return;
    }
    fetchTree(selectedTreeId);
  }, [selectedTreeId, fetchTree]);

  useEffect(() => {
    if (!open) {
      return;
    }
    fetchQuestionTrees({
      PageNumber: pageNumber,
      PageSize: pageSize,
      name: searchValueDebounced,
    });
  }, [pageNumber, searchValueDebounced, open, fetchQuestionTrees]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handleSelectTree = useCallback(
    (tree: TreeMeta) => {
      onSelect(tree.id);
      setOpen(false);
    },
    [onSelect],
  );

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
    [handleSelectTree],
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
        {treeLoading && <Loader />}
        {treeError && (
          <Typography size="m" error>
            {treeError}
          </Typography>
        )}
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
            error={error}
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
