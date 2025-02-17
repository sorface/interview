import React, { FunctionComponent } from 'react';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { ParticipantReactions } from './ParticipantReactions';
import { ParticipantPinButton } from './ParticipantPinButton';
import { viewerPinOrder } from './VideoChat';

interface VideochatParticipantWithoutVideoProps {
  order?: number;
  avatar?: string;
  nickname?: string;
  reaction?: string | null;
  pinable?: boolean;
  handleUserPin?: () => void;
}

export const VideochatParticipantWithoutVideo: FunctionComponent<
  VideochatParticipantWithoutVideoProps
> = ({ order, avatar, nickname, reaction, pinable, handleUserPin }) => {
  const orderSafe = order || 2;

  return (
    <div
      className="videochat-participant-viewer-wrapper"
      style={{ order: orderSafe }}
    >
      <div className="videochat-participant-viewer">
        {avatar && <UserAvatar src={avatar} nickname={nickname || ''} />}
        <div>{nickname}</div>
        {pinable && handleUserPin && (
          <ParticipantPinButton
            handlePin={handleUserPin}
            pin={orderSafe === viewerPinOrder}
          />
        )}
        <ParticipantReactions reaction={reaction} />
      </div>
    </div>
  );
};
