import classNames from 'classnames';
import React from 'react';
import { TreeNode } from 'versatile-tree';
import { TreeController, UNDEFINED_ID } from './hooks/useTreeController';
import { BasicTreeNodeTitleComponent } from './BasicTreeNodeTitleComponent';
import { BasicTreeNodeDropdown } from './BasicTreeNodeDropdown';
import { key } from './utils/utils';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';

export interface BasicTreeNodeComponentProps {
  node: TreeNode;
  treeController: TreeController;
}

export const BasicTreeNodeComponent = ({
  node,
  treeController,
}: BasicTreeNodeComponentProps) => {
  const expanded = treeController.expansions.isExpandedNode(node);
  const children: TreeNode[] = node.getChildren() ?? [];
  const hasChildren = node.hasChildren();

  const handleToggleShowChildren = () => {
    if (node.hasChildren()) {
      treeController.expansions.toggleExpandNode(node);
    }
    treeController.focus.setFocusedNode(node);
  };

  React.useEffect(() => {
    // Ensure ancestors of focused items are all expanded
    if (
      treeController.focus.focusedNode &&
      node.isAncestorOf(treeController.focus.focusedNode) &&
      !treeController.expansions.isExpandedNode(node)
    ) {
      treeController.expansions.toggleExpandNode(node, true);
    } else if (
      treeController.filters.hasFilter &&
      treeController.filters.isFilterAncestor(node) &&
      !treeController.expansions.isExpandedNode(node)
    ) {
      treeController.expansions.toggleExpandNode(node, true);
    }
  }, [
    node,
    treeController.expansions,
    treeController.filters,
    treeController.focus.focusedNode,
  ]);

  const childrenToRender = treeController.filters.hasFilter
    ? children.filter(
        (child) =>
          treeController.filters.isFilteredNode(child) ||
          treeController.filters.isFilterAncestor(child),
      )
    : children;
  const entryElements: JSX.Element[] = childrenToRender.map((childNode, i) => {
    const elemKey = key(
      `parent`,
      node.getData()[treeController.options.idPropertyName] ?? UNDEFINED_ID,
      'child',
      childNode.getData()[treeController.options.idPropertyName] ??
        UNDEFINED_ID,
      'index',
      i,
    );
    return (
      <BasicTreeNodeComponent
        key={elemKey}
        node={childNode}
        treeController={treeController}
      />
    );
  });

  return (
    <div className="flex flex-col">
      {!node.isRoot() && (
        <div className="flex items-center">
          {hasChildren && (
            <div
              className="flex items-center cursor-pointer"
              onClick={handleToggleShowChildren}
            >
              {hasChildren && (
                <Icon
                  size="m"
                  name={
                    expanded ? IconNames.ChevronDown : IconNames.ChevronForward
                  }
                />
              )}
            </div>
          )}
          <BasicTreeNodeTitleComponent
            node={node}
            treeController={treeController}
          />
          <div className="d-flex align-items-center gap-1 ms-1 user-select-none">
            <BasicTreeNodeDropdown
              node={node}
              treeController={treeController}
            />
          </div>
        </div>
      )}
      {((expanded && hasChildren) || node.isRoot()) && (
        <div
          className={classNames(
            'flex flex-col gap-1 mt-1 ml-1',
            !node.isRoot() && 'ms-4',
          )}
        >
          {entryElements.length > 0 && (
            <div className="flex flex-col gap-1">{entryElements}</div>
          )}
        </div>
      )}
    </div>
  );
};
