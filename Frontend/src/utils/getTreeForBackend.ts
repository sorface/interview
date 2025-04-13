import { Tree } from 'versatile-tree';
import { TreeNodeType, TreeNode } from '../types/tree';

const appendToTreeForBackend = (tree: Tree, treeForBackend: TreeNode[]) => {
  const data = tree.getData();
  tree.getChildren().forEach((treeChild, index) => {
    const dataChild = treeChild.getData();
    treeForBackend.push({
      id: dataChild.id,
      parentQuestionSubjectTreeId: data.id,
      question: dataChild.question,
      type: TreeNodeType.Question,
      order: index,
    });
    appendToTreeForBackend(treeChild, treeForBackend);
  });
};

export const getTreeForBackend = (
  tree: Tree,
  rootNodeFakeId: string,
  rootNodeName?: string,
): TreeNode[] => {
  const result: TreeNode[] = [];
  appendToTreeForBackend(tree, result);
  result.push({
    id: rootNodeFakeId,
    parentQuestionSubjectTreeId: null,
    question: {
      id: null,
      value: rootNodeName || '',
    },
    type: TreeNodeType.Empty,
    order: 0,
  });
  return result.map((node) => ({
    ...node,
    parentQuestionSubjectTreeId:
      node.id === rootNodeFakeId
        ? node.parentQuestionSubjectTreeId
        : node.parentQuestionSubjectTreeId || rootNodeFakeId,
  }));
};
