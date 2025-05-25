import React, { FunctionComponent } from 'react';
import { MilestoneRect } from './MilestoneRect';
import { MilestoneTree } from './MilestoneTree';
import { useThemeClassName } from '../../../hooks/useThemeClassName';
import { Theme } from '../../../context/ThemeContext';
import { Gap } from '../../../components/Gap/Gap';
import { getTreeProgress } from '../utils/getTreeProgress';
import { notAvailableId } from '../Roadmap';

interface MilestoneTreeItem {
  id: string;
  name?: string;
  questionTreeId?: string;
}

interface MilestoneProps {
  name: string;
  arrow?: boolean;
  trees: MilestoneTreeItem[];
  onCreateRoom: (treeId: string, treeName: string) => void;
}

export const Milestone: FunctionComponent<MilestoneProps> = ({
  name,
  arrow,
  trees,
  onCreateRoom,
}) => {
  const lineStroke = useThemeClassName({
    [Theme.Dark]: 'border-dark-grey4',
    [Theme.Light]: 'border-text-light',
  });
  const arrowStroke = useThemeClassName({
    [Theme.Dark]: 'border-dark-grey4',
    [Theme.Light]: 'border-text-light',
  });

  const sumProgress = trees.reduce((accum, currTree) => {
    const progress = getTreeProgress(currTree.id);
    if (progress !== 100) {
      return accum;
    }
    return accum + getTreeProgress(currTree.id);
  }, 0);
  const totalProgress = ~~(sumProgress / trees.length);

  const handleCreateRoom = (tree: MilestoneTreeItem) => () => {
    onCreateRoom(tree.questionTreeId || '', tree.name || '');
  };

  return (
    <div className="relative flex flex-col items-start">
      {arrow && (
        <div
          className={`absolute left-[0.5rem] top-[3.5rem] w-[2px] h-[calc(100%-4.25rem)] border-l-[4px] border-dotted ${lineStroke}`}
        >
          <div
            style={{
              width: 0,
              height: 0,
              margin: '12px auto',
              borderLeft: '8px solid transparent',
              borderRight: '8px solid transparent',
              borderTopWidth: '12px',
              borderTopStyle: 'solid',
            }}
            className={`absolute bottom-[-14px] left-[-10px] ${arrowStroke}`}
          ></div>
        </div>
      )}
      <MilestoneRect caption={`${name} (${totalProgress}%)`} />
      <Gap sizeRem={0.5} />
      <ul className="pl-[1.5rem] flex flex-col items-start">
        {trees.map((tree) => (
          <MilestoneTree
            key={tree.id}
            id={tree.id}
            name={tree.name || ''}
            notAvailable={tree.id === notAvailableId}
            onCreate={handleCreateRoom(tree)}
          />
        ))}
      </ul>
      <Gap sizeRem={2.25} />
    </div>
  );
};
