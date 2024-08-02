import { FunctionComponent } from 'react';
import { Room } from '../../types/room';
import { UserAvatar } from '../UserAvatar/UserAvatar';

import './RoomParticipants.css';

interface RoomParticipantsProps {
  participants: Room['participants'];
}

export const RoomParticipants: FunctionComponent<RoomParticipantsProps> = ({
  participants,
}) => {
  return (
    <div className='room-participants'>
      {participants.map(roomParticipant => (
        <div className='room-participant'>
          <UserAvatar
            src={roomParticipant.avatar}
            nickname={roomParticipant.nickname}
          />
        </div>
      ))}
    </div>
  );
};
