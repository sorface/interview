import { FunctionComponent } from 'react';
import { Room } from '../../types/room';
import { Typography } from '../Typography/Typography';
import { RoomDateAndTime } from '../RoomDateAndTime/RoomDateAndTime';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Gap } from '../Gap/Gap';
import { Link } from 'react-router-dom';
import { getRoomLink } from '../../utils/getRoomLink';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface RoomsHistoryItemProps {
  room: Room;
}

export const RoomsHistoryItem: FunctionComponent<RoomsHistoryItemProps> = ({
  room,
}) => {
  const roomLink = getRoomLink(room);
  const themeClassName = useThemeClassName({
    [Theme.Dark]: 'hover:bg-dark-history-hover',
    [Theme.Light]: 'hover:bg-form',
  });

  const iconClassName = useThemeClassName({
    [Theme.Dark]: 'text-dark-grey4',
    [Theme.Light]: 'text-grey3',
  });

  return (
    <div className={`p-0.5 cursor-pointer rounded-0.375 ${themeClassName}`}>
      <Link to={roomLink} className='no-underline'>
        <div className='flex items-center justify-between'>
          <RoomDateAndTime
            scheduledStartTime={room.scheduledStartTime}
            typographySize='s'
            timer={room.timer}
            mini
            secondary
          />
          <div className={`flex items-center ${iconClassName}`}>
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
