import { FunctionComponent, useEffect, useState } from 'react';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';
import { padTime } from '../../../../utils/padTime';

const updateIntervalMs = 1000;

const formatTime = (timeMs: number) => {
  const hours = Math.floor((timeMs / (1000 * 60 * 60)));
  const minutes = Math.floor((timeMs / 1000 / 60) % 60);
  const seconds = Math.floor((timeMs / 1000) % 60);
  return `${padTime(hours)}:${padTime(minutes)}:${padTime(seconds)}`;
};

interface RoomTimerProps {
  startTime: string;
  durationSec: number;
}

export const RoomTimer: FunctionComponent<RoomTimerProps> = ({
  startTime,
  durationSec,
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
    <div className="flex items-center justify-center px-1 py-0.625 bg-wrap rounded-2">
      <Icon name={IconNames.Time} />
      <Gap sizeRem={0.25} horizontal />
      <div className='w-4 text-left'>
        {formatTime(remainingTimeMs <= 0 ? 0 : remainingTimeMs)}
      </div>
    </div>
  );
};
