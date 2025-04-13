import React, { FunctionComponent, ReactNode } from 'react';
import { ParticipantReactions } from './ParticipantReactions';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';

interface VideochatParticipantWithVideoProps {
  order?: number;
  children: ReactNode;
  nickname?: string;
  reaction?: string | null;
}

export const VideochatParticipantWithVideo: FunctionComponent<
  VideochatParticipantWithVideoProps
> = ({ order, children, nickname, reaction }) => {
  const orderSafe = order || 2;
  return (
    <div
      className={`videochat-participant ${orderSafe === 1 ? 'videochat-participant-big' : 'videochat-participant'}`}
      style={{ order: orderSafe }}
    >
      <div className="videochat-participant-name-wrapper">
        <div className="videochat-caption videochat-participant-name">
          <Typography size="s">{nickname}</Typography>
          <Gap sizeRem={0.25} horizontal />
          <Icon size="s" name={IconNames.MicOn} />
          {!!reaction && <ParticipantReactions reaction={reaction} />}
        </div>
      </div>
      <div className="h-full">{children}</div>
    </div>
  );
};
