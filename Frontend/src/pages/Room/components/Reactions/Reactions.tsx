import { FunctionComponent, useCallback, useEffect, useState } from 'react';
import { ReactionsList } from '../../../../components/ReactionsList/ReactionsList';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { Reaction } from '../../../../types/reaction';
import {
  PaginationUrlParams,
  SendEventBody,
  SendReactionBody,
  eventApiDeclaration,
  reactionsApiDeclaration,
  roomReactionApiDeclaration,
  roomsApiDeclaration,
} from '../../../../apiDeclarations';
import { Room } from '../../../../types/room';
import { Event } from '../../../../types/event';
import { UserType } from '../../../../types/user';
import { Loader } from '../../../../components/Loader/Loader';
import { useReactionsFeed } from '../../hooks/useReactionsFeed';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { EventsState } from '../../hooks/useEventsState';

import './Reactions.css';

const reactionsPageSize = 30;
const reactionsPageNumber = 1;

const eventToReaction = (event: Event): Reaction => ({
  id: event.id,
  type: {
    id: event.id,
    name: event.type,
    value: 0,
  }
});

export interface ReactionsProps {
  room: Room | null;
  eventsState: EventsState;
  roles: string[];
  participantType: UserType | null;
  lastWsMessage: MessageEvent<any> | null;
}

export const Reactions: FunctionComponent<ReactionsProps> = ({
  room,
  eventsState,
  roles,
  participantType,
  lastWsMessage,
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

  const {
    apiMethodState: apiGetEventState,
    fetchData: fetchRoomEvents,
  } = useApiMethod<Event[], PaginationUrlParams>(eventApiDeclaration.get);
  const {
    process: { loading: loadingRoomEvent, error: errorRoomEvent },
    data: events,
  } = apiGetEventState;

  const {
    apiMethodState: apiSendEventState,
    fetchData: sendRoomEvent,
  } = useApiMethod<unknown, SendEventBody>(roomsApiDeclaration.sendEvent);
  const {
    process: { loading: loadingSendRoomEvent, error: errorSendRoomEvent },
  } = apiSendEventState;

  const { reactionsFeed } = useReactionsFeed({
    lastMessage: lastWsMessage,
  });
  const [lastSendedReactionType, setLastSendedReactionType] = useState('');

  const reactionsSafe = reactions || [];

  const eventsReationsFiltered =
    !events ?
      [] :
      events
        .filter(event =>
          event.roles.some(role => roles.includes(role)) &&
          participantType &&
          event.participantTypes.includes(participantType)
        )
        .map(eventToReaction);


  useEffect(() => {
    fetchReactions({
      PageSize: reactionsPageSize,
      PageNumber: reactionsPageNumber,
    });
    fetchRoomEvents({
      PageSize: reactionsPageSize,
      PageNumber: reactionsPageNumber,
    });
  }, [room?.id, fetchReactions, fetchRoomEvents]);

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

  const handleEventClick = useCallback((event: Reaction) => {
    if (!room || !eventsState) {
      throw new Error('Error sending reaction. Room not found.');
    }
    const prevEnabled = Boolean(eventsState[event.type.name]);
    sendRoomEvent({
      roomId: room.id,
      type: event.type.name,
      additionalData: { value: !prevEnabled },
    });
    setLastSendedReactionType(event.type.name);
  }, [room, eventsState, sendRoomEvent]);

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
    <div className='reactions'>
      <ReactionsList
        sortOrder={-1}
        reactions={reactionsSafe}
        loadingReactionName={loadingRoomReaction ? lastSendedReactionType : null}
        reactionsFeed={reactionsFeed}
        onClick={handleReactionClick}
      />
      <ReactionsList
        sortOrder={1}
        reactions={eventsReationsFiltered}
        loadingReactionName={loadingSendRoomEvent ? lastSendedReactionType : null}
        reactionsFeed={reactionsFeed}
        onClick={handleEventClick}
      />
      {errorRoomReaction && <div>{localizationCaptions[LocalizationKey.ErrorSendingReaction]}</div>}
      {loadingRoomEvent && <div>{localizationCaptions[LocalizationKey.GetRoomEvent]}...</div>}
      {errorRoomEvent && <div>{localizationCaptions[LocalizationKey.ErrorGetRoomEvent]}</div>}
      {errorSendRoomEvent && <div>{localizationCaptions[LocalizationKey.ErrorSendingRoomEvent]}</div>}
    </div>
  );
};
