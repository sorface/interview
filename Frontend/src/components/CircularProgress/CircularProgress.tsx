import { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { useRandomId } from '../../hooks/useRandomId';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Tooltip } from '../Tooltip/Tooltip';

import './CircularProgress.css';

interface CircularProgressProps {
  value: number | null;
  caption: string | number | null;
  size: 'm' | 's';
}

export const CircularProgress: FunctionComponent<CircularProgressProps> = ({
  value,
  caption,
  size,
}) => {
  const randomId = useRandomId();
  const localizationCaptions = useLocalizationCaptions();
  const sizePx = size === 'm' ? 108 : 40;
  const strokeWidth = size === 'm' ? '12px' : '4px';

  if (value === null) {
    return (
      <>
        <Tooltip
          id={`CircularProgress-tooltip-${randomId}`}
          place='top'
          content={localizationCaptions[LocalizationKey.RoomReviewWaiting]}
        />
        <div
          data-tooltip-id={`CircularProgress-tooltip-${randomId}`}
          className='flex items-center justify-center'
          style={{
            width: `${sizePx}px`,
            height: `${sizePx}px`,
            fontSize: `${sizePx}px`,
          }}
        >
          <Icon inheritFontSize name={IconNames.Hourglass} />
        </div>
      </>
    );
  }

  return (
    <div
      className='relative circular-progress-wrapper'
      style={{ '--size': `${sizePx}px` } as React.CSSProperties}
    >
      <svg
        width={`${sizePx + 1}`}
        height={`${sizePx + 1}`}
        viewBox={`0 0 ${sizePx + 1} ${sizePx + 1}`}
        className='circular-progress'
        style={{
          '--end-progress': value,
          '--stroke-width': strokeWidth,
        } as React.CSSProperties}
      >
        <circle stroke='var(--blue-light)'></circle>
        <circle stroke='var(--blue-main)' className='fg'></circle>
      </svg>
      <div className='circular-progress-caption'>
        <Typography size={size === 'm' ? 'xxxl' : 'm'}>{caption}</Typography>
      </div>
    </div>
  );
};
