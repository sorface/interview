import { FunctionComponent } from 'react';
import { Room } from '../../types/room';
import { UserAvatar } from '../UserAvatar/UserAvatar';
import { Tooltip } from 'react-tooltip';
import { useParticipantTypeLocalization } from '../../hooks/useParticipantTypeLocalization';

import './RoomParticipants.css';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface RoomParticipantsProps {
  participants: Room['participants'];
}

export const RoomParticipants: FunctionComponent<RoomParticipantsProps> = ({
  participants,
}) => {
  const localizeParticipantType = useParticipantTypeLocalization();
  const tooltipThemedBackground = useThemeClassName({
    [Theme.Dark]: 'var(--dark-dark1)',
    [Theme.Light]: 'var(--dark)',
  });

  return (
    <div className='room-participants'>
      {participants.map(roomParticipant => (
        <div key={roomParticipant.id} className='room-participant'>
          <Tooltip
            id={`user-tooltip-${roomParticipant.id}`}
            place='top'
            style={{ backgroundColor: tooltipThemedBackground, zIndex: 999 }}
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
