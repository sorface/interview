import React, { FunctionComponent } from 'react';
import { Typography } from '../../../components/Typography/Typography';
import { getTreeProgress } from '../utils/getTreeProgress';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';
import { Gap } from '../../../components/Gap/Gap';

interface MilestoneTreeProps {
  id: string;
  name: string;
  onCreate: () => void;
}

export const MilestoneTree: FunctionComponent<MilestoneTreeProps> = ({
  id,
  name,
  onCreate,
}) => {
  const treeCompleted = getTreeProgress(id) === 100;

  return (
    <div className="hover:underline hover:cursor-pointer" onClick={onCreate}>
      <Typography size="l" semibold success={treeCompleted}>
        <div className="flex items-center">
          {treeCompleted && (
            <Icon name={IconNames.CheckmarkDone} inheritFontSize />
          )}
          <Gap sizeRem={0.25} horizontal />
          {name}
        </div>
      </Typography>
    </div>
  );
};
