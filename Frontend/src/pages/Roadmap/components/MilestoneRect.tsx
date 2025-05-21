import React, { FunctionComponent } from 'react';
import { useThemeClassName } from '../../../hooks/useThemeClassName';
import { Theme } from '../../../context/ThemeContext';
import { Typography } from '../../../components/Typography/Typography';

interface MilestoneRectProps {
  caption: string;
}

export const MilestoneRect: FunctionComponent<MilestoneRectProps> = ({
  caption,
}) => {
  const fill = useThemeClassName({
    [Theme.Dark]: 'bg-dark-active',
    [Theme.Light]: 'bg-grey1',
  });
  const stroke = useThemeClassName({
    [Theme.Dark]: 'border-dark-grey4',
    [Theme.Light]: 'border-text-light',
  });

  return (
    <div
      className={`w-full rounded-[0.75rem] border-[0.158rem] py-[0.75rem] ${fill} ${stroke}`}
    >
      <Typography size="l" bold>
        {caption}
      </Typography>
    </div>
  );
};
