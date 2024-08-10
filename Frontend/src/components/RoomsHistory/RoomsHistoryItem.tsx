import { FunctionComponent } from 'react';
import { Room } from '../../types/room';
import { Typography } from '../Typography/Typography';
import { RoomDateAndTime } from '../RoomDateAndTime/RoomDateAndTime';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Gap } from '../Gap/Gap';
import { Link } from 'react-router-dom';
import { getRoomLink } from '../../utils/getRoomLink';

interface RoomsHistoryItemProps {
  room: Room;
}

export const RoomsHistoryItem: FunctionComponent<RoomsHistoryItemProps> = ({
  room,
}) => {
  const roomLink = getRoomLink(room);

  return (
    <div className='p-0.5 cursor-pointer hover:bg-form rounded-0.375'>
      <Link to={roomLink} className='no-underline'>
        <div className='flex items-center justify-between'>
          <RoomDateAndTime
            scheduledStartTime={room.scheduledStartTime}
            typographySize='s'
            timer={room.timer}
            mini
            secondary
          />
          <div className='flex items-center opacity-0.5'>
            <Icon name={IconNames.ChevronForward} size='s' />
          </div>
        </div>
        <Gap sizeRem={0.25} />
        <Typography size='s'>
          {room.name}
        </Typography>
      </Link>
    </div>
  );
};
