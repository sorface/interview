import React, { FunctionComponent, useEffect, useState } from 'react';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';
import { padTime } from '../../../../utils/padTime';
import { Typography } from '../../../../components/Typography/Typography';

const updateIntervalMs = 1000;

const formatTime = (timeMs: number, mini?: boolean) => {
  const hours = Math.floor(timeMs / (1000 * 60 * 60));
  const minutes = mini
    ? Math.floor(timeMs / 1000 / 60)
    : Math.floor((timeMs / 1000 / 60) % 60);
  const seconds = Math.floor((timeMs / 1000) % 60);
  if (mini) {
    return `${padTime(minutes)}:${padTime(seconds)}`;
  }
  return `${padTime(hours)}:${padTime(minutes)}:${padTime(seconds)}`;
};

interface RoomTimerProps {
  startTime: string;
  durationSec: number;
  mini?: boolean;
}

export const RoomTimer: FunctionComponent<RoomTimerProps> = ({
  startTime,
  durationSec,
  mini,
}) => {
  const [remainingTimeMs, setRemainingTimeMs] = useState(0);

  useEffect(() => {
    if (!startTime || !durationSec) {
      return;
    }
    const endDate = new Date(startTime);
    endDate.setSeconds(endDate.getSeconds() + durationSec);

    const updateTimer = () => {
      setRemainingTimeMs(endDate.getTime() - Date.now());
    };
    updateTimer();
    const intervalId = setInterval(updateTimer, updateIntervalMs);

    return () => {
      clearInterval(intervalId);
    };
  }, [startTime, durationSec]);

  return (
    <div
      className={`flex items-center justify-center px-1 py-0.625 rounded-2 ${mini ? '' : 'bg-wrap'}`}
    >
      <Icon size={mini ? 's' : 'm'} name={IconNames.Time} />
      <Gap sizeRem={0.25} horizontal />
      <div
        className={`${mini ? 'w-2.3125' : 'w-4'} text-left whitespace-nowrap`}
      >
        <Typography size={mini ? 'm' : 'l'}>
          {formatTime(remainingTimeMs <= 0 ? 0 : remainingTimeMs, mini)}
        </Typography>
      </div>
    </div>
  );
};
