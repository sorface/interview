import { FunctionComponent } from "react";
import { IconNames, reactionIcon } from "../../../../constants";
import { ThemedIcon } from "../ThemedIcon/ThemedIcon";

import './ParticipantReactions.css';

interface ParticipantReactionsProps {
  reaction?: string | null;
};

const defaultIconName = IconNames.None;

export const ParticipantReactions: FunctionComponent<ParticipantReactionsProps> = ({
  reaction,
}) => {
  return (
    <div className="participant-reactions">
      {!!reaction && (
        <span key={reaction} className="participant-reaction">
          <ThemedIcon name={reactionIcon[reaction] || defaultIconName} />
        </span>
      )}
    </div>
  );
};
