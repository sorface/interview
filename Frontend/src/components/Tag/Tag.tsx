import { FunctionComponent, ReactNode } from 'react';
import { Typography } from '../Typography/Typography';

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

export const Tag: FunctionComponent<TagProps> = ({
  state,
  children,
}) => {
  return (
    <div
      className={`flex items-center py-0.125 px-0.375 rounded-0.375 bg-tag-${state}`}
    >
      <Typography size='s'>
        {children}
      </Typography>
    </div>
  )
};
