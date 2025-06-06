import React from 'react';
import { TreeNode } from 'versatile-tree';
import { TreeController } from './hooks/useTreeController';
import { ContextMenu } from '../ContextMenu/ContextMenu';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

export interface BasicTreeNodeDropdownProps {
  node: TreeNode;
  treeController: TreeController;
}

export const BasicTreeNodeDropdown = ({
  node,
  treeController,
}: BasicTreeNodeDropdownProps) => {
  const localizationCaptions = useLocalizationCaptions();

  const handleMoveLeft = React.useCallback(() => {
    treeController.mutations.moveNodeLeft(node);
  }, [treeController.mutations, node]);

  const handleMoveDown = React.useCallback(() => {
    treeController.mutations.moveNodeDown(node);
  }, [treeController.mutations, node]);

  const handleMoveUp = React.useCallback(() => {
    treeController.mutations.moveNodeUp(node);
  }, [treeController.mutations, node]);

  const handleMoveRight = React.useCallback(() => {
    treeController.mutations.moveNodeRight(node);
  }, [treeController.mutations, node]);

  const handleNewItemBelow = () => {
    const newNodeData = treeController.options.createNewData();
    const newNode = treeController.mutations.addSiblingNodeData(
      node,
      newNodeData,
    );
    treeController.focus.setFocusedNode(newNode);
  };

  const handleNewItemInside = () => {
    const newNodeData = treeController.options.createNewData();
    const newNode = treeController.mutations.addChildNodeData(
      node,
      newNodeData,
    );
    treeController.focus.setFocusedNode(newNode);
  };

  const handleEllipsisMouseDown = () => {
    treeController.mutations.deleteNode(node);
  };

  return (
    <ContextMenu
      translateRem={{ x: 0, y: 0 }}
      toggleContent={<Icon name={IconNames.EllipsisVertical} />}
    >
      <ContextMenu.Item
        key="1"
        title={localizationCaptions[LocalizationKey.QuestionTreeNodeDelete]}
        onClick={() => handleEllipsisMouseDown()}
      />
      <ContextMenu.Item
        key="2"
        title={
          localizationCaptions[LocalizationKey.QuestionTreeNodeNewItemBelow]
        }
        onClick={handleNewItemBelow}
      />
      <ContextMenu.Item
        key="3"
        title={
          localizationCaptions[LocalizationKey.QuestionTreeNodeNewItemInside]
        }
        onClick={handleNewItemInside}
      />
      <ContextMenu.Item
        key="4"
        title={localizationCaptions[LocalizationKey.QuestionTreeNodeMoveUp]}
        onClick={handleMoveUp}
      />
      <ContextMenu.Item
        key="5"
        title={localizationCaptions[LocalizationKey.QuestionTreeNodeMoveDown]}
        onClick={handleMoveDown}
      />
      <ContextMenu.Item
        key="6"
        title={localizationCaptions[LocalizationKey.QuestionTreeNodeMoveLeft]}
        onClick={handleMoveLeft}
      />
      <ContextMenu.Item
        key="7"
        title={localizationCaptions[LocalizationKey.QuestionTreeNodeMoveRight]}
        onClick={handleMoveRight}
      />
    </ContextMenu>
  );
};
