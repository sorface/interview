import { FunctionComponent, ReactNode } from 'react';
import { VideochatParticipantWithVideo } from './VideochatParticipantWithVideo';
import { VideochatParticipantWithoutVideo } from './VideochatParticipantWithoutVideo';

interface VideochatParticipantProps {
  viewer: boolean;
  order?: number;
  children: ReactNode;
  avatar?: string;
  nickname?: string;
  reaction?: string | null;
}

export const VideochatParticipant: FunctionComponent<VideochatParticipantProps> = ({
  viewer,
  ...restProps
}) => {
  if (viewer) {
    return (
      <VideochatParticipantWithoutVideo {...restProps} />
    );
  }
  return (
    <VideochatParticipantWithVideo {...restProps} />
  );
};
