import { FunctionComponent, ReactNode } from 'react';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { ParticipantReactions } from './ParticipantReactions';

interface VideochatParticipantWithVideoProps {
  order?: number;
  children: ReactNode;
  avatar?: string;
  nickname?: string;
  videoTrackEnabled?: boolean;
  reaction?: string | null;
}

export const VideochatParticipantWithVideo: FunctionComponent<VideochatParticipantWithVideoProps> = ({
  order,
  children,
  avatar,
  nickname,
  videoTrackEnabled,
  reaction,
}) => {
  const orderSafe = order || 2;
  return (
    <div
      className={`videochat-participant ${orderSafe === 1 ? 'videochat-participant-big' : 'videochat-participant'}`}
      style={{ order: orderSafe }}
    >
      {!!reaction && (
        <div className='videochat-caption videochat-overlay videochat-participant-reactions'>
          <ParticipantReactions reaction={reaction} />
        </div>
      )}
      <div className='videochat-caption videochat-overlay videochat-participant-name'>
        {avatar && (
          <UserAvatar
            src={avatar}
            nickname={nickname || ''}
          />
        )}
        {nickname}
      </div>
      {videoTrackEnabled === false && (
        avatar ? (
          <div className='avatar-wrapper-no-video'>
            <UserAvatar
              src={avatar}
              nickname={nickname || ''}
            />
          </div>
        ) : (
          <div>NO VIDEO</div>
        )
      )}
      <div style={{ ...(videoTrackEnabled === false && { display: 'none' }) }}>
        {children}
      </div>
    </div>
  );
};
