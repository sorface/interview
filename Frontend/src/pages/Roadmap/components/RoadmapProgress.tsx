import React, { FunctionComponent } from 'react';
import { Typography } from '../../../components/Typography/Typography';
import { Gap } from '../../../components/Gap/Gap';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';

interface RoadmapProgressProps {
  levelCaption: LocalizationKey;
  levelProgressPercent: number;
}

export const RoadmapProgress: FunctionComponent<RoadmapProgressProps> = ({
  levelCaption,
  levelProgressPercent,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  return (
    <div className="flex flex-col text-left bg-wrap p-[1rem] rounded-[0.5rem]">
      <Typography size="l" bold>
        {localizationCaptions[LocalizationKey.RoadmapYourProgress]}
      </Typography>
      <Gap sizeRem={0.75} />
      <div className="flex items-center justify-between">
        <Typography size="m" bold>
          {localizationCaptions[levelCaption]}
        </Typography>
        <Typography size="m" secondary>
          {levelProgressPercent}%
        </Typography>
      </div>
      <Gap sizeRem={0.75} />
      <div className="flex">
        <progress
          max={100}
          value={levelProgressPercent}
          className="rounded-progress flex-1"
        ></progress>
      </div>
      <Gap sizeRem={0.75} />
      <Typography size="m" secondary>
        {100 - levelProgressPercent}%{' '}
        {localizationCaptions[LocalizationKey.RoadmapToFinish]}
      </Typography>
    </div>
  );
};
