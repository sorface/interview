import React, { FunctionComponent, useContext } from 'react';
import { Typography } from '../../../../components/Typography/Typography';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';
import { DeviceContext } from '../../../../context/DeviceContext';

interface QuestionsProgressProps {
  value: number;
}

export const QuestionsProgress: FunctionComponent<QuestionsProgressProps> = ({
  value,
}) => {
  const device = useContext(DeviceContext);
  const localizationCaptions = useLocalizationCaptions();
  const progressThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-grey4',
    [Theme.Light]: 'bg-blue-light',
  });

  return (
    <div
      className="relative bg-wrap"
      style={{
        width: device === 'Desktop' ? '18.9375rem' : '9.46875rem',
        clipPath: 'inset(0% 0% 0% 0% round 2rem)',
      }}
    >
      <div
        className={`absolute h-full move-transition ${progressThemedClassName}`}
        style={{ width: `${value}%` }}
      ></div>
      <div className="absolute h-full flex items-center px-[1.25rem]">
        <Typography size="m" semibold>
          {localizationCaptions[LocalizationKey.Completed]} {value}%
        </Typography>
      </div>
    </div>
  );
};
