import { FunctionComponent, useContext } from 'react';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { Room } from '../../types/room';
import { LocalizationContext } from '../../context/LocalizationContext';
import { padTime } from '../../utils/padTime';

interface RoomDateAndTimeProps {
  scheduledStartTime: string;
  timer?: Room['timer'];
  mini?: boolean;
}

const formatScheduledStartTime = (scheduledStartTime: string, lang: string) => {
  const date = new Date(scheduledStartTime);
  const month = date.toLocaleString(lang, { month: 'long' });
  return `${date.getDate()} ${month}`;
};

const formatScheduledStartDay = (scheduledStartTime: string, lang: string) => {
  const date = new Date(scheduledStartTime);
  const day = date.toLocaleString(lang, { weekday: 'short' });
  return `${day}`;
};

const formatTime = (value: Date) => `${padTime(value.getHours())}:${padTime(value.getMinutes())}`;

const formatDuration = (scheduledStartTime: string, durationSec: number) => {
  const dateStart = new Date(scheduledStartTime);
  const dateEnd = new Date(scheduledStartTime);
  dateEnd.setSeconds(dateEnd.getSeconds() + durationSec);
  return `${formatTime(dateStart)} - ${formatTime(dateEnd)}`;
};

export const RoomDateAndTime: FunctionComponent<RoomDateAndTimeProps> = ({
  scheduledStartTime,
  timer,
  mini,
}) => {
  const { lang } = useContext(LocalizationContext);

  return (
    <div className={`flex  ${!mini ? 'justify-between' : ''} items-baseline`}>
      <div className='flex items-baseline'>
        <Typography size='s'>
          {formatScheduledStartTime(scheduledStartTime, lang)}
        </Typography>
        <Gap sizeRem={0.5} horizontal />
        <div className='capitalize opacity-0.5'>
          <Typography size='s'>
            {formatScheduledStartDay(scheduledStartTime, lang)}
          </Typography>
        </div>
      </div>
      {timer && (
        <>
          {mini && (<Gap sizeRem={1} horizontal />)}
          <Typography size='s'>
            {formatDuration(scheduledStartTime, timer.durationSec)}
          </Typography>
        </>
      )}
    </div>
  );
};
