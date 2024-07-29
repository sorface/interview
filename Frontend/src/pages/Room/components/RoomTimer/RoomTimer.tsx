import { FunctionComponent, useEffect, useState } from 'react';
import { Room } from '../../../../types/room';
import { ThemedIcon } from '../ThemedIcon/ThemedIcon';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';

const updateIntervalMs = 1000;

const padTime = (value: number) => String(value).padStart(2, '0');

const formatTime = (timeMs: number) => {
  const hours = Math.floor((timeMs / (1000 * 60 * 60)));
  const minutes = Math.floor((timeMs / 1000 / 60) % 60);
  const seconds = Math.floor((timeMs / 1000) % 60);
  return `${padTime(hours)}:${padTime(minutes)}:${padTime(seconds)}`;
};

export const RoomTimer: FunctionComponent<Required<Room['timer']>> = ({
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
      <ThemedIcon name={IconNames.Time} />
      <Gap sizeRem={0.25} horizontal />
      <div className='w-4 text-left'>
        {formatTime(remainingTimeMs <= 0 ? 0 : remainingTimeMs)}
      </div>
    </div>
  );
};
