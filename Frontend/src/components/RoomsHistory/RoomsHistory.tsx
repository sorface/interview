import React, { FunctionComponent } from 'react';
import { Room } from '../../types/room';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Typography } from '../Typography/Typography';
import { LocalizationKey } from '../../localization';
import { Gap } from '../Gap/Gap';
import { RoomsHistoryItem } from './RoomsHistoryItem';

interface RoomsHistoryProps {
  rooms: Room[];
}
export const RoomsHistory: FunctionComponent<RoomsHistoryProps> = ({
  rooms,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  return (
    <div className="invisible-scroll text-left overflow-auto flex flex-col h-full px-[0.625rem] bg-wrap rounded-[1.125rem]">
      <Gap sizeRem={1.5} />
      <div className="sticky top-0 bg-wrap w-full z-1">
        <Typography size="m" semibold>
          {localizationCaptions[LocalizationKey.InterviewHistoryTitle]}
        </Typography>
        <Gap sizeRem={0.5} />
      </div>
      <div>
        {rooms.map((room) => (
          <RoomsHistoryItem key={room.id} room={room} />
        ))}
      </div>
      {rooms.length === 0 && (
        <div className="h-full flex flex-col items-center justify-center">
          <Typography size="m" secondary>
            {localizationCaptions[LocalizationKey.NoRecords]}
          </Typography>
        </div>
      )}
      <Gap sizeRem={0.5} />
    </div>
  );
};
