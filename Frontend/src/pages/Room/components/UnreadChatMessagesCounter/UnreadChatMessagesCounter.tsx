import React, { FunctionComponent } from 'react';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';
import { Typography } from '../../../../components/Typography/Typography';

interface UnreadChatMessagesCounterProps {
  value: number | string;
}

export const UnreadChatMessagesCounter: FunctionComponent<
  UnreadChatMessagesCounterProps
> = ({ value }) => {
  const themedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-red',
    [Theme.Light]: 'bg-red text-white',
  });
  return (
    <div
      className={`${themedClassName} flex items-center justify-center w-[0.75rem] h-[0.75rem] p-[0.25rem] rounded-[0.75rem]`}
    >
      <Typography size="s">{value}</Typography>
    </div>
  );
};
