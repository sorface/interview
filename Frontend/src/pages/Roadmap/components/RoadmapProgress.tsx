import React, { FunctionComponent } from 'react';
import { Typography } from '../../../components/Typography/Typography';
import { Gap } from '../../../components/Gap/Gap';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';

interface RoadmapProgressProps {
  level: number;
  levelCaption: LocalizationKey;
  levelProgressPercent: number;
}

export const RoadmapProgress: FunctionComponent<RoadmapProgressProps> = ({
  level,
  levelCaption,
  levelProgressPercent,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  return (
    <div className="flex flex-col bg-wrap py-[1rem] px-[0.75rem] rounded-[0.5rem]">
      <Typography size="m" semibold>
        {localizationCaptions[LocalizationKey.RoadmapYourProgress]}
      </Typography>
      <Gap sizeRem={0.5} />
      <div className="flex">
        <progress
          max={100}
          value={levelProgressPercent}
          className="flex-1"
        ></progress>
      </div>
      <Gap sizeRem={0.5} />
      <Typography size="m">
        {localizationCaptions[LocalizationKey.RoadmapLevel]} {level}:{' '}
        {localizationCaptions[levelCaption]}
      </Typography>
    </div>
  );
};
