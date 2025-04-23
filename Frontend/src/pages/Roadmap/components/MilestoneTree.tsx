import React, { FunctionComponent } from 'react';
import { Typography } from '../../../components/Typography/Typography';

interface MilestoneTreeProps {
  name: string;
}

export const MilestoneTree: FunctionComponent<MilestoneTreeProps> = ({
  name,
}) => {
  return (
    <div className="hover:underline hover:cursor-pointer">
      <Typography size="l" semibold>
        • {name}
      </Typography>
    </div>
  );
};
