import React, { FunctionComponent } from 'react';
import { ParticipantReactions } from './ParticipantReactions';
import { Typography } from '../../../../components/Typography/Typography';
import { Gap } from '../../../../components/Gap/Gap';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';

interface VideochatParticipantWithoutVideoProps {
  order?: number;
  nickname?: string;
  reaction?: string | null;
}

export const VideochatParticipantWithoutVideo: FunctionComponent<
  VideochatParticipantWithoutVideoProps
> = ({ order, nickname, reaction }) => {
  const orderSafe = order || 2;
  const viewerThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-dark2',
    [Theme.Light]: 'bg-grey-active',
  });
  return (
    <div
      className={`videochat-participant ${orderSafe === 1 ? 'videochat-participant-big' : 'videochat-participant'}`}
      style={{ order: orderSafe }}
    >
      <div
        className={`h-full flex flex-col items-center justify-center rounded-1.25 ${viewerThemedClassName}`}
      >
        <div className="flex items-center">
          <Typography size="xxl">{nickname}</Typography>
          {!!reaction && <ParticipantReactions reaction={reaction} />}
        </div>
        <Gap sizeRem={0.6875} />
        <Icon size="s" name={IconNames.MicOff} />
      </div>
    </div>
  );
};
