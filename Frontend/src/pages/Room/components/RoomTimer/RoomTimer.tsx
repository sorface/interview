import { FunctionComponent, useEffect, useState } from 'react';
import { Room } from '../../../../types/room';
import { ThemedIcon } from '../ThemedIcon/ThemedIcon';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';

import './RoomTimer.css';

const updateIntervalMs = 33 * 1000;

const padTime = (value: number) => String(value).padStart(2, '0');

const formatTime = (timeMs: number) => {
  const hours = Math.floor((timeMs / (1000 * 60 * 60)));
  const minutes = Math.floor((timeMs / 1000 / 60) % 60);
  return `${padTime(hours)}:${padTime(minutes)}`;
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
    const intervalId = setInterval(updateTimer, updateIntervalMs);

    return () => {
      clearInterval(intervalId);
    };
  }, [startTime, durationSec]);

  return (
    <div className="room-timer flex items-center bg-wrap rounded-2">
      <ThemedIcon name={IconNames.Time} />
      <Gap sizeRem={0.25} horizontal />
      {formatTime(remainingTimeMs <= 0 ? 0 : remainingTimeMs)}
    </div>
  );
};
