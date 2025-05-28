import React, { FunctionComponent } from 'react';
import { Typography } from '../../../components/Typography/Typography';
import { getTreeProgress } from '../utils/getTreeProgress';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';
import { Gap } from '../../../components/Gap/Gap';
import { useThemeClassName } from '../../../hooks/useThemeClassName';
import { Theme } from '../../../context/ThemeContext';

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
  const iconBg = useThemeClassName({
    [Theme.Dark]: 'bg-dark-dark1',
    [Theme.Light]: 'bg-millestone-item-light',
  });
  const iconBgCompleted = useThemeClassName({
    [Theme.Dark]: 'bg-dark-green',
    [Theme.Light]: 'bg-green',
  });

  return (
    <li className="hover:underline hover:cursor-pointer" onClick={onCreate}>
      <div className="flex pb-[1rem]">
        <div
          className={`${treeCompleted ? iconBgCompleted : iconBg} w-[2.5rem] h-[2.5rem] flex items-center justify-center rounded-[0.5rem]`}
        >
          <Icon
            size="s"
            name={
              treeCompleted ? IconNames.Checkmark : IconNames.ChevronForward
            }
          />
        </div>
        <Gap sizeRem={1} horizontal />
        <div className="flex items-center justify-center">
          <Typography size="l" semibold success={treeCompleted}>
            {name}
          </Typography>
        </div>
      </div>
    </li>
  );
};
