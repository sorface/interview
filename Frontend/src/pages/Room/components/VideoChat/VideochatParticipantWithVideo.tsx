import React, { FunctionComponent, ReactNode } from 'react';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { ParticipantReactions } from './ParticipantReactions';
import { ParticipantPinButton } from './ParticipantPinButton';
import { viewerPinOrder } from './VideoChat';

interface VideochatParticipantWithVideoProps {
  order?: number;
  children: ReactNode;
  avatar?: string;
  nickname?: string;
  reaction?: string | null;
  pinable?: boolean;
  handleUserPin?: () => void;
}

export const VideochatParticipantWithVideo: FunctionComponent<
  VideochatParticipantWithVideoProps
> = ({
  order,
  children,
  avatar,
  nickname,
  reaction,
  pinable,
  handleUserPin,
}) => {
  const orderSafe = order || 2;
  const pin = orderSafe === viewerPinOrder;

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
      <div
        className={`videochat-caption videochat-overlay videochat-participant-pin ${pin ? '' : 'opacity-0'} hover:opacity-100`}
      >
        {pinable && handleUserPin && (
          <ParticipantPinButton handlePin={handleUserPin} pin={pin} />
        )}
      </div>
      <div className="videochat-caption videochat-overlay videochat-participant-name">
        {avatar && <UserAvatar src={avatar} nickname={nickname || ''} />}
        {nickname}
      </div>
      <div className="h-full">{children}</div>
    </div>
  );
};
