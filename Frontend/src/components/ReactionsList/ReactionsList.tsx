import React, { Fragment, FunctionComponent, useCallback } from 'react';
import { Reaction } from '../../types/reaction';
import { EventName, IconNames, reactionIcon } from '../../constants';
import { RoomToolsPanel } from '../../pages/Room/components/RoomToolsPanel/RoomToolsPanel';
import { Gap } from '../Gap/Gap';

import './ReactionsList.css';

const defaultIconName = IconNames.None;

const ignoredReactions: string[] = [
  EventName.CodeEditorLanguage,
  EventName.CodeEditorCursor,
];

interface ReactionsListProps {
  reactions: Reaction[];
  loadingReactionName?: string | null;
  sortOrder: 1 | -1;
  onClick: (reaction: Reaction) => void;
}

export const ReactionsList: FunctionComponent<ReactionsListProps> = ({
  reactions,
  loadingReactionName,
  sortOrder,
  onClick,
}) => {
  const handleReactionClick = useCallback((reaction: Reaction) => () => {
    onClick(reaction);
  }, [onClick]);

  return (
    <>
      {reactions
        .filter(reaction => !ignoredReactions.includes(reaction.type.name))
        .sort((reaction1, reaction2) => {
          if (reaction1.type.name > reaction2.type.name) {
            return 1 * sortOrder;
          }
          if (reaction1.type.name < reaction2.type.name) {
            return -1 * sortOrder;
          }
          return 0;
        })
        .map((reaction, index, reacts) => (
          <Fragment key={reaction.id}>
            <RoomToolsPanel.SwitchButton
              key={`${reaction.id}${reaction.type.name}`}
              enabled={true}
              loading={reaction.type.name === loadingReactionName}
              iconEnabledName={reactionIcon[reaction.type.name] || defaultIconName}
              iconDisabledName={reactionIcon[reaction.type.name] || defaultIconName}
              onClick={handleReactionClick(reaction)}
            />
            {index !== reacts.length - 1 && (<Gap sizeRem={0.125} />)}
          </Fragment>
        ))}
    </>
  );
};
