import React, { FunctionComponent } from 'react';
import { IconNames, reactionIcon } from '../../../../constants';
import { Icon } from '../Icon/Icon';

import './ParticipantReactions.css';

interface ParticipantReactionsProps {
  reaction?: string | null;
}

const defaultIconName = IconNames.None;

export const ParticipantReactions: FunctionComponent<
  ParticipantReactionsProps
> = ({ reaction }) => {
  return (
    <div className="participant-reactions">
      {!!reaction && (
        <span key={reaction} className="participant-reaction">
          <Icon size="s" name={reactionIcon[reaction] || defaultIconName} />
        </span>
      )}
    </div>
  );
};
