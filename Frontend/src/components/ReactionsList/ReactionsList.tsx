import React, { FunctionComponent, useCallback } from 'react';
import { Reaction } from '../../types/reaction';
import { SwitchButton } from '../../pages/Room/components/VideoChat/SwitchButton';
import { EventName, IconNames, reactionIcon } from '../../constants';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { ReactionsFeed } from '../../pages/Room/hooks/useReactionsFeed';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

import './ReactionsList.css';

const defaultIconName = IconNames.None;

const ignoredReactions: string[] = [
  EventName.CodeEditorLanguage,
  EventName.CodeEditorCursor,
];

interface ReactionsListProps {
  reactions: Reaction[];
  reactionsFeed: ReactionsFeed;
  loadingReactionName?: string | null;
  sortOrder: 1 | -1;
  onClick: (reaction: Reaction) => void;
}

export const ReactionsList: FunctionComponent<ReactionsListProps> = ({
  reactions,
  reactionsFeed,
  loadingReactionName,
  sortOrder,
  onClick,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const reactionLocalization: Record<string, string> = {
    Like: localizationCaptions[LocalizationKey.Like],
    Dislike: localizationCaptions[LocalizationKey.Dislike],
    CodeEditor: localizationCaptions[LocalizationKey.CodeEditor],
  }

  const handleReactionClick = useCallback((reaction: Reaction) => () => {
    onClick(reaction);
  }, [onClick]);

  return (
    <div className='reactions-list'>
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
        .map(reaction => (
          <div key={`${reaction.id}${reaction.type.name}`}>
            {!!reactionsFeed[reaction.type.name] && (
              <div key={reactionsFeed[reaction.type.name]} className='reaction-feed-item'>
                <ThemedIcon name={reactionIcon[reaction.type.name] || IconNames.None} />
              </div>
            )}
            <SwitchButton
              enabled={true}
              loading={reaction.type.name === loadingReactionName}
              iconEnabledName={reactionIcon[reaction.type.name] || defaultIconName}
              iconDisabledName={reactionIcon[reaction.type.name] || defaultIconName}
              subCaption={reactionLocalization[reaction.type.name] || reaction.type.name}
              onClick={handleReactionClick(reaction)}
            />
          </div>
        ))}
    </div>
  );
};
