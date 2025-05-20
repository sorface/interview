import React, { FunctionComponent } from 'react';
import { Typography } from '../../../components/Typography/Typography';
import { getTreeProgress } from '../utils/getTreeProgress';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';
import { Gap } from '../../../components/Gap/Gap';

interface MilestoneTreeProps {
  id: string;
  name: string;
  notAvailable?: boolean;
  onCreate: () => void;
}

export const MilestoneTree: FunctionComponent<MilestoneTreeProps> = ({
  id,
  name,
  notAvailable,
  onCreate,
}) => {
  const treeCompleted = getTreeProgress(id) === 100;

  return (
    <li
      className={`${notAvailable ? 'hover:cursor-not-allowed' : 'hover:underline hover:cursor-pointe'}`}
      onClick={notAvailable ? undefined : onCreate}
    >
      <Typography
        size="l"
        semibold
        success={!notAvailable && treeCompleted}
        secondary={notAvailable}
      >
        <div className="flex items-center">
          {treeCompleted && (
            <Icon name={IconNames.CheckmarkDone} inheritFontSize />
          )}
          <Gap sizeRem={0.25} horizontal />
          {name}
        </div>
      </Typography>
    </li>
  );
};
