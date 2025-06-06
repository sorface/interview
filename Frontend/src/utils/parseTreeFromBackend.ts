import { QuestionsTree } from '../types/questionsTree';
import { TreeNode } from '../types/tree';

const findChildrenNodesInTreeFromBackend = (
  node: TreeNode,
  treeFromBackend: TreeNode[],
): TreeNode[] => {
  const children = treeFromBackend
    .sort((tNode1, tNode2) => tNode1.order - tNode2.order)
    .filter((tNode) => tNode.parentQuestionSubjectTreeId === node.id)
    .map((tNode) => ({
      ...tNode,
      children: findChildrenNodesInTreeFromBackend(tNode, treeFromBackend),
    }));
  return children;
};

export const parseTreeFromBackend = (treeFromBackend: QuestionsTree) => {
  const rootNode = treeFromBackend.tree.find(
    (node) => node.id === treeFromBackend.rootQuestionSubjectTreeId,
  );
  if (!rootNode) {
    console.warn('no rootNode in parseTreeFromBackend');
    return [];
  }
  return findChildrenNodesInTreeFromBackend(rootNode, treeFromBackend.tree);
};
