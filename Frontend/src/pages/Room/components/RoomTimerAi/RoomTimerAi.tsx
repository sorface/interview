import React, { FunctionComponent } from 'react';
import { RoomTimer } from '../RoomTimer/RoomTimer';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';

interface RoomTimerAiProps {
  startTime: string;
  durationSec: number;
}

export const RoomTimerAi: FunctionComponent<RoomTimerAiProps> = ({
  startTime,
  durationSec,
}) => {
  const progressThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-active',
    [Theme.Light]: 'bg-blue-ai-room-timer',
  });

  return (
    <div
      className={`${progressThemedClassName} rounded-[6.25rem] flex justify-center items-center`}
    >
      <RoomTimer startTime={startTime} durationSec={durationSec} mini />
    </div>
  );
};
