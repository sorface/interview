import React from 'react';
import { Tree, TreeNode } from 'versatile-tree';
import { defaultTreeControllerOptions } from './TreeControllerOptions';
import { AnyObject } from '../../../types/anyObject';

export const defaultTreeData = {
  [defaultTreeControllerOptions.titlePropertyName]: 'root',
  children: [],
};

type UseTreeStateReturnValue = [
  Tree,
  (tree: TreeNode | Tree) => void,
  (treeData: AnyObject) => void,
];

export const useTreeState = (initial: AnyObject): UseTreeStateReturnValue => {
  const [treeDataStored, setTreeData] = React.useState(initial);
  const treeData = React.useMemo(
    () => (!treeDataStored ? defaultTreeData : treeDataStored),
    [treeDataStored],
  );
  const tree = React.useMemo(() => new Tree(treeData), [treeData]);
  const setTree = React.useCallback(
    (newTree: Tree | TreeNode) => {
      setTreeData(newTree.toObject());
    },
    [setTreeData],
  );

  return React.useMemo(
    () => [tree, setTree, setTreeData],
    [setTree, setTreeData, tree],
  );
};

export type TreeState = ReturnType<typeof useTreeState>;
