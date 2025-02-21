import React, {
  FunctionComponent,
  useState,
  useEffect,
  ChangeEvent,
} from 'react';
import { Button } from '../../components/Button/Button';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Gap } from '../../components/Gap/Gap';
import {
  defaultTreeControllerOptions,
  TreeControllerOptions,
} from '../../components/TreeEditor/hooks/TreeControllerOptions';
import {
  defaultTreeData,
  useTreeState,
} from '../../components/TreeEditor/hooks/useTreeState';
import {
  TreeController,
  useTreeController,
} from '../../components/TreeEditor/hooks/useTreeController';
import { BasicTreeNodeComponent } from '../../components/TreeEditor/BasicTreeNodeComponent';
import { TreeViewer } from '../../components/TreeViewer/TreeViewer';
import { useApiMethod } from '../../hooks/useApiMethod';
import {
  CreateQuestionTreeBody,
  questionTreeApiDeclaration,
} from '../../apiDeclarations';
import { useNavigate, useParams } from 'react-router-dom';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Loader } from '../../components/Loader/Loader';
import { Typography } from '../../components/Typography/Typography';
import toast from 'react-hot-toast';
import { IconNames, pathnames } from '../../constants';
import { Icon } from '../Room/components/Icon/Icon';
import { getTreeForBackend } from '../../utils/getTreeForBackend';
import { parseTreeFromBackend } from '../../utils/parseTreeFromBackend';
import { QuestionsTree } from '../../types/questionsTree';

interface QuestionTreeCreateProps {
  edit: boolean;
}

export const QuestionTreeCreate: FunctionComponent<QuestionTreeCreateProps> = ({
  edit,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [rootNodeFakeId] = useState(crypto.randomUUID());
  const [name, setName] = useState('');
  const { id } = useParams();
  const navigate = useNavigate();
  const [displayTreeViewer, setDisplayTreeViewer] = useState(!edit);
  const treeOptions: TreeControllerOptions = defaultTreeControllerOptions;
  const [tree, setTree, setTreeData] = useTreeState({ ...defaultTreeData });
  const treeController: TreeController = useTreeController(
    tree,
    setTree,
    treeOptions,
  );

  const { apiMethodState: getState, fetchData: fetchGet } = useApiMethod<
    QuestionsTree,
    string
  >(questionTreeApiDeclaration.get);
  const {
    process: { loading: getLoading, error: getError },
    data: getedTree,
  } = getState;

  const { apiMethodState: upsertState, fetchData: fetchUpsert } = useApiMethod<
    unknown,
    CreateQuestionTreeBody
  >(questionTreeApiDeclaration.upsert);
  const {
    process: { loading: upsertLoading, error: upsertError },
    data: upsertedTree,
  } = upsertState;

  const totalError = getError || upsertError;

  useEffect(() => {
    if (!getedTree) {
      return;
    }
    const parsed = parseTreeFromBackend(getedTree);
    setName(getedTree.name);
    setTreeData({
      ...defaultTreeData,
      children: parsed,
    });
    setDisplayTreeViewer(true);
  }, [getedTree]);

  useEffect(() => {
    if (!upsertedTree) {
      return;
    }
    toast.success(
      edit
        ? localizationCaptions[LocalizationKey.QuestionTreeUpdatedSuccessfully]
        : localizationCaptions[LocalizationKey.QuestionTreeCreatedSuccessfully],
    );
    navigate(pathnames.questionTrees);
  }, [upsertedTree, edit, localizationCaptions, navigate]);

  const handleCategoryOrderChange = (event: ChangeEvent<HTMLInputElement>) => {
    setName(event.target.value);
  };

  useEffect(() => {
    if (!edit) {
      return;
    }
    if (!id) {
      throw new Error('Category id not found');
    }
    fetchGet(id);
  }, [edit, id, fetchGet]);

  // Ensure there's always at least one item to edit
  React.useEffect(() => {
    if (!treeController.tree.hasChildren()) {
      const newNodeData = treeController.options.createNewData();
      const node = treeController.mutations.addChildNodeData(
        treeController.tree,
        newNodeData,
      );
      treeController.focus.setFocusedNode(node);
    }
  }, [
    treeController.focus,
    treeController.mutations,
    treeController.tree,
    treeController.options,
  ]);

  const handleExpandAll = () => {
    treeController.expansions.expandAll();
  };

  const handleCollapseAll = () => {
    treeController.expansions.collapseAll();
  };

  const handleCreate = () => {
    const treeForBackend = getTreeForBackend(tree, rootNodeFakeId);
    const treeId = edit ? getedTree?.id : crypto.randomUUID();
    if (!treeId) {
      throw new Error('empty treeId in handleCreate');
    }
    fetchUpsert({
      id: treeId,
      name: name,
      order: 0,
      parentQuestionTreeId: null,
      tree: treeForBackend.map((node) => ({
        ...node,
        question: undefined,
        questionId: node.question?.id || null,
      })),
    });
  };

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.QuestionTreesPageName]}
      />
      <div className="flex items-center">
        <label htmlFor="name">
          {localizationCaptions[LocalizationKey.QuestionTreeName]}:
        </label>
        <Gap sizeRem={0.5} horizontal />
        <input
          id="name"
          name="name"
          type="text"
          size={30}
          value={name}
          onChange={handleCategoryOrderChange}
        />
        <Button className="ml-auto" onClick={handleCreate}>
          {localizationCaptions[LocalizationKey.Save]}
        </Button>
      </div>
      <Gap sizeRem={0.75} />
      <div className="flex items-center">
        <Button variant="text" onClick={handleExpandAll}>
          {localizationCaptions[LocalizationKey.QuestionTreeExpandAll]}
          <Gap sizeRem={0.5} horizontal />
          <Icon name={IconNames.Expand} />
        </Button>
        <Button variant="text" onClick={handleCollapseAll}>
          {localizationCaptions[LocalizationKey.QuestionTreeCollapseAll]}
          <Gap sizeRem={0.5} horizontal />
          <Icon name={IconNames.Collapse} />
        </Button>
      </div>
      <div className="flex flex-col">
        {getLoading || (upsertLoading && <Loader />)}
        {totalError && (
          <Typography error size="m">
            {totalError}
          </Typography>
        )}
        <div className="">
          <div className="flex">
            <BasicTreeNodeComponent
              node={treeController.tree}
              treeController={treeController}
            />
          </div>
        </div>
        <Gap sizeRem={1.75} />
        <div style={{ width: '100%', height: '500px' }}>
          {displayTreeViewer && (
            <TreeViewer
              tree={getTreeForBackend(
                tree,
                rootNodeFakeId,
                localizationCaptions[LocalizationKey.QuestionTreeRootNode],
              )}
            />
          )}
        </div>
      </div>
    </>
  );
};
