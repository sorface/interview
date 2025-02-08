import React, { FunctionComponent, ReactNode } from 'react';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { ParticipantReactions } from './ParticipantReactions';
import { ParticipantPinButton } from './ParticipantPinButton';
import { usePin } from './hoks/usePin';

interface VideochatParticipantWithVideoProps {
  order?: number;
  children: ReactNode;
  avatar?: string;
  nickname?: string;
  reaction?: string | null;
  pinable?: boolean;
}

export const VideochatParticipantWithVideo: FunctionComponent<
  VideochatParticipantWithVideoProps
> = ({ order, children, avatar, nickname, reaction, pinable }) => {
  const { handlePin, pin, orderSafe } = usePin(order);

  return (
    <div
      className={`videochat-participant ${orderSafe === 1 ? 'videochat-participant-big' : 'videochat-participant'}`}
      style={{ order: orderSafe }}
    >
      {!!reaction && (
        <div className="videochat-caption videochat-overlay videochat-participant-reactions">
          <ParticipantReactions reaction={reaction} />
        </div>
      )}
      <div className="videochat-caption videochat-overlay videochat-participant-name">
        {avatar && <UserAvatar src={avatar} nickname={nickname || ''} />}
        {nickname}
        {pinable && <ParticipantPinButton handlePin={handlePin} pin={pin} />}
      </div>
      <div className="h-full">{children}</div>
    </div>
  );
};
