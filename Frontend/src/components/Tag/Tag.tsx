import React, { FunctionComponent, ReactNode } from 'react';
import { Typography, TypographyProps } from '../Typography/Typography';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme, ThemeInUi } from '../../context/ThemeContext';

export enum TagState {
  Waiting = 'waiting',
  Pending = 'pending',
  Closed = 'closed',
  WaitingForAction = 'waitingForAction',
}

interface TagProps {
  state: TagState;
  typographySize?: TypographyProps['size'];
  typographySemibold?: TypographyProps['semibold'];
  children: ReactNode;
}

const themeClassNames: Record<ThemeInUi, Record<TagState, string>> = {
  [Theme.Dark]: {
    [TagState.Waiting]: 'bg-dark-blue-0.25 text-dark-blue-light',
    [TagState.Pending]: 'bg-dark-green-0.25 text-dark-green-light',
    [TagState.Closed]: 'bg-dark-closed text-dark-closed-light',
    [TagState.WaitingForAction]: 'bg-dark-orange-0.25 text-dark-orange-light',
  },
  [Theme.Light]: {
    [TagState.Waiting]: 'bg-blue-light',
    [TagState.Pending]: 'bg-green-light',
    [TagState.Closed]: 'bg-grey-active',
    [TagState.WaitingForAction]: 'bg-red-light',
  },
};

export const Tag: FunctionComponent<TagProps> = ({
  state,
  typographySize,
  typographySemibold,
  children,
}) => {
  const themeClassName = useThemeClassName(themeClassNames)[state];
  return (
    <div
      className={`flex items-center py-[0.125rem] px-[0.375rem] rounded-[0.375rem] ${themeClassName}`}
    >
      <Typography size={typographySize || 's'} semibold={typographySemibold}>
        {children}
      </Typography>
    </div>
  );
};
