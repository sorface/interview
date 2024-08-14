import { FunctionComponent, ReactNode } from 'react';
import { Typography } from '../Typography/Typography';
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
  }
};

export const Tag: FunctionComponent<TagProps> = ({
  state,
  children,
}) => {
  const themeClassName = useThemeClassName(themeClassNames)[state];
  return (
    <div
      className={`flex items-center py-0.125 px-0.375 rounded-0.375 ${themeClassName}`}
    >
      <Typography size='s'>
        {children}
      </Typography>
    </div>
  )
};
