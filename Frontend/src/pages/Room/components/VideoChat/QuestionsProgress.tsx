import React, { FunctionComponent } from 'react';
import { Typography } from '../../../../components/Typography/Typography';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';

interface QuestionsProgressProps {
  value: number;
}

export const QuestionsProgress: FunctionComponent<QuestionsProgressProps> = ({
  value,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const progressThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-grey4',
    [Theme.Light]: 'bg-blue-light',
  });

  return (
    <div
      className="relative bg-wrap"
      style={{ width: '18.9375rem', clipPath: 'inset(0% 0% 0% 0% round 2rem)' }}
    >
      <div
        className={`absolute h-full move-transition ${progressThemedClassName}`}
        style={{ width: `${value}%` }}
      ></div>
      <div className="absolute h-full flex items-center px-1.25">
        <Typography size="m" semibold>
          {localizationCaptions[LocalizationKey.Completed]} {value}%
        </Typography>
      </div>
    </div>
  );
};
