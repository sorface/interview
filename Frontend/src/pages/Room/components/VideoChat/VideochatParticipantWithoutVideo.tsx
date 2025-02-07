import React, { FunctionComponent } from 'react';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { ParticipantReactions } from './ParticipantReactions';
import { usePin } from './hoks/usePin';
import { ParticipantPinButton } from './ParticipantPinButton';

interface VideochatParticipantWithoutVideoProps {
  order?: number;
  avatar?: string;
  nickname?: string;
  reaction?: string | null;
  pinable?: boolean;
}

export const VideochatParticipantWithoutVideo: FunctionComponent<
  VideochatParticipantWithoutVideoProps
> = ({ order, avatar, nickname, reaction, pinable }) => {
  const { handlePin, pin, orderSafe } = usePin(order);

  return (
    <div
      className="videochat-participant-viewer-wrapper"
      style={{ order: orderSafe }}
    >
      {pinable && <ParticipantPinButton handlePin={handlePin} pin={pin} />}

      <div className="videochat-participant-viewer">
        {avatar && <UserAvatar src={avatar} nickname={nickname || ''} />}
        <div>{nickname}</div>
        <ParticipantReactions reaction={reaction} />
      </div>
    </div>
  );
};
