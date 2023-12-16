import { FunctionComponent } from 'react';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { ParticipantReactions } from './ParticipantReactions';

interface VideochatParticipantWithoutVideoProps {
  order?: number;
  avatar?: string;
  nickname?: string;
  reaction?: string | null;
}

export const VideochatParticipantWithoutVideo: FunctionComponent<VideochatParticipantWithoutVideoProps> = ({
  order,
  avatar,
  nickname,
  reaction,
}) => {
  const orderSafe = order || 2;
  return (
    <div
      className='videochat-participant-viewer-wrapper'
      style={{ order: orderSafe }}
    >
      <div
        className='videochat-participant-viewer'
      >
        {avatar && (
          <UserAvatar
            src={avatar}
            nickname={nickname || ''}
          />
        )}
        <div>{nickname}</div>
        <ParticipantReactions reaction={reaction} />
      </div>
    </div>
  );
};
