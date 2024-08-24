import { FunctionComponent, useCallback, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { ReactionsList } from '../../../../components/ReactionsList/ReactionsList';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { Reaction } from '../../../../types/reaction';
import {
  PaginationUrlParams,
  SendReactionBody,
  reactionsApiDeclaration,
  roomReactionApiDeclaration,
} from '../../../../apiDeclarations';
import { Room } from '../../../../types/room';
import { Loader } from '../../../../components/Loader/Loader';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

const reactionsPageSize = 30;
const reactionsPageNumber = 1;

export interface ReactionsProps {
  room: Room | null;
}

export const Reactions: FunctionComponent<ReactionsProps> = ({
  room,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const {
    apiMethodState: apiReactionsState,
    fetchData: fetchReactions,
  } = useApiMethod<Reaction[], PaginationUrlParams>(reactionsApiDeclaration.getPage);
  const {
    process: { loading: loadingReactions, error: errorReactions },
    data: reactions,
  } = apiReactionsState;

  const {
    apiMethodState: apiRoomReactionState,
    fetchData: sendRoomReaction,
  } = useApiMethod<unknown, SendReactionBody>(roomReactionApiDeclaration.send);
  const {
    process: { loading: loadingRoomReaction, error: errorRoomReaction },
  } = apiRoomReactionState;

  const [lastSendedReactionType, setLastSendedReactionType] = useState('');

  const reactionsSafe = reactions || [];

  useEffect(() => {
    fetchReactions({
      PageSize: reactionsPageSize,
      PageNumber: reactionsPageNumber,
    });
  }, [room?.id, fetchReactions]);

  useEffect(() => {
    if (!errorRoomReaction) {
      return;
    }
    toast.error(localizationCaptions[LocalizationKey.ErrorSendingReaction]);
  }, [errorRoomReaction, localizationCaptions]);

  const handleReactionClick = useCallback((reaction: Reaction) => {
    if (!room) {
      throw new Error('Error sending reaction. Room not found.');
    }
    sendRoomReaction({
      reactionId: reaction.id,
      roomId: room.id,
      payload: reaction.type.name,
    });
    setLastSendedReactionType(reaction.type.name);
  }, [room, sendRoomReaction]);

  if (errorReactions) {
    return (
      <div>{localizationCaptions[LocalizationKey.ReactionsLoadingError]}: {errorReactions}</div>
    );
  }
  if (loadingReactions) {
    return (
      <Loader />
    );
  }

  return (
    <>
      <ReactionsList
        sortOrder={-1}
        reactions={reactionsSafe}
        loadingReactionName={loadingRoomReaction ? lastSendedReactionType : null}
        onClick={handleReactionClick}
      />
    </>
  );
};
