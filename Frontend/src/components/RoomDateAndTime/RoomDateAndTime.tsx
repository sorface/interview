import { FunctionComponent, useContext } from 'react';
import { Typography, TypographyProps } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { Room } from '../../types/room';
import { LocalizationContext } from '../../context/LocalizationContext';
import { padTime } from '../../utils/padTime';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface RoomDateAndTimeProps {
  scheduledStartTime: string;
  typographySize: TypographyProps['size'];
  timer?: Room['timer'];
  mini?: boolean;
  secondary?: boolean;
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
  typographySize,
  timer,
  mini,
  secondary,
}) => {
  const { lang } = useContext(LocalizationContext);
  const themeSecondaryClassName = useThemeClassName({
    [Theme.Dark]: 'text-dark-grey4',
    [Theme.Light]: 'text-grey3',
  });
  const secondaryClassName = secondary ? themeSecondaryClassName : '';

  return (
    <div className={`flex  ${!mini ? 'justify-between' : ''} ${secondaryClassName} items-baseline`}>
      <div className='flex items-baseline'>
        <Typography size={typographySize}>
          {formatScheduledStartTime(scheduledStartTime, lang)}
        </Typography>
        <Gap sizeRem={0.5} horizontal />
        <div className={`capitalize ${!secondary ? themeSecondaryClassName : ''}`}>
          <Typography size={typographySize}>
            {formatScheduledStartDay(scheduledStartTime, lang)}
          </Typography>
        </div>
      </div>
      {timer && (
        <>
          {mini && (<Gap sizeRem={1} horizontal />)}
          <Typography size={typographySize}>
            {formatDuration(scheduledStartTime, timer.durationSec)}
          </Typography>
        </>
      )}
    </div>
  );
};
