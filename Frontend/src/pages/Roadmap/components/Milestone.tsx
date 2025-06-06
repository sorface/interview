import React, { FunctionComponent } from 'react';
import { MilestoneTree } from './MilestoneTree';
import { Gap } from '../../../components/Gap/Gap';
import { Typography } from '../../../components/Typography/Typography';

interface MilestoneTreeItem {
  id: string;
  name?: string;
  questionTreeId?: string;
  roomId?: string;
}

interface MilestoneProps {
  name: string;
  trees: MilestoneTreeItem[];
  onCreateRoom: (treeId: string, treeName: string) => void;
  onRoomAlreadyExists: (roomId: string) => void;
}

export const Milestone: FunctionComponent<MilestoneProps> = ({
  name,
  trees,
  onCreateRoom,
  onRoomAlreadyExists,
}) => {
  const handleCreateRoom = (tree: MilestoneTreeItem) => () => {
    if (tree.roomId) {
      onRoomAlreadyExists(tree.roomId);
      return;
    }
    onCreateRoom(tree.questionTreeId || '', tree.name || '');
  };

  return (
    <div className="relative flex flex-col items-start pt-[0.75rem]">
      <Typography size="xl" bold>
        {name}
      </Typography>
      <Gap sizeRem={1.25} />
      <ul className="flex flex-col items-start">
        {trees.map((tree) => (
          <MilestoneTree
            key={tree.id}
            id={tree.questionTreeId || ''}
            name={tree.name || ''}
            onCreate={handleCreateRoom(tree)}
          />
        ))}
      </ul>
    </div>
  );
};
