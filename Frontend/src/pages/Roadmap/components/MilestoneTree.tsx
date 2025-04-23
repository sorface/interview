import React, { FunctionComponent } from 'react';
import { Typography } from '../../../components/Typography/Typography';

interface MilestoneTreeProps {
  name: string;
  onCreate: () => void;
}

export const MilestoneTree: FunctionComponent<MilestoneTreeProps> = ({
  name,
  onCreate,
}) => {
  return (
    <div className="hover:underline hover:cursor-pointer" onClick={onCreate}>
      <Typography size="l" semibold>
        • {name}
      </Typography>
    </div>
  );
};
