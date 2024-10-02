import { FunctionComponent } from 'react';
import { Room } from '../../types/room';
import { UserAvatar } from '../UserAvatar/UserAvatar';
import { useParticipantTypeLocalization } from '../../hooks/useParticipantTypeLocalization';
import { Tooltip } from '../Tooltip/Tooltip';

import './RoomParticipants.css';

interface RoomParticipantsProps {
  participants: Room['participants'];
}

export const RoomParticipants: FunctionComponent<RoomParticipantsProps> = ({
  participants,
}) => {
  const localizeParticipantType = useParticipantTypeLocalization();

  return (
    <div className='room-participants'>
      {participants.map(roomParticipant => (
        <div key={roomParticipant.id} className='room-participant'>
          <Tooltip
            id={`user-tooltip-${roomParticipant.id}`}
            place='top'
            content={`${roomParticipant.nickname} (${localizeParticipantType(roomParticipant.type)})`}
          />
          <UserAvatar
            data-tooltip-id={`user-tooltip-${roomParticipant.id}`}
            src={roomParticipant.avatar}
            nickname={roomParticipant.nickname}
          />
        </div>
      ))}
    </div>
  );
};
