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
import { Room, RoomState, RoomStateAdditionalStatefulPayload } from '../../../../types/room';
import { Event } from '../../../../types/event';
import { UserType } from '../../../../types/user';
import { Loader } from '../../../../components/Loader/Loader';
import { useReactionsFeed } from '../../hooks/useReactionsFeed';
import { Localization } from '../../../../localization';

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

type ParsedStates = Record<string, boolean>;

export interface ReactionsProps {
  room: Room | null;
  roomState: RoomState | null;
  roles: string[];
  participantType: UserType | null;
  lastWsMessage: MessageEvent<any> | null;
}

export const Reactions: FunctionComponent<ReactionsProps> = ({
  room,
  roomState,
  roles,
  participantType,
  lastWsMessage,
}) => {
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
  const [parsedStates, setParsedStates] = useState<ParsedStates>({});
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

  useEffect(() => {
    if (!roomState) {
      return;
    }
    const parsedStates: ParsedStates = {};
    roomState.states.forEach(roomState =>
      parsedStates[roomState.type] = (JSON.parse(roomState.payload) as RoomStateAdditionalStatefulPayload).enabled
    );
    setParsedStates(parsedStates);
  }, [roomState]);

  useEffect(() => {
    if (!lastWsMessage || !parsedStates) {
      return;
    }
    try {
      const parsedData = JSON.parse(lastWsMessage?.data);
      if (!parsedData?.Stateful) {
        return;
      }
      const stateType = parsedData.Type;
      const stateValue = (parsedData.Value.AdditionalData as RoomStateAdditionalStatefulPayload).enabled;
      const oldValue = parsedStates[stateType];
      if (oldValue !== stateValue) {
        setParsedStates({
          ...parsedStates,
          [stateType]: stateValue,
        });
      }
    } catch {
    }
  }, [lastWsMessage, parsedStates]);

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
    if (!room || !parsedStates) {
      throw new Error('Error sending reaction. Room not found.');
    }
    const prevEnabled = Boolean(parsedStates[event.type.name]);
    sendRoomEvent({
      roomId: room.id,
      type: event.type.name,
      additionalData: { enabled: !prevEnabled },
    });
    setLastSendedReactionType(event.type.name);
  }, [room, parsedStates, sendRoomEvent]);

  if (errorReactions) {
    return (
      <div>{Localization.ReactionsLoadingError}: {errorReactions}</div>
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
      {errorRoomReaction && <div>{Localization.ErrorSendingReaction}</div>}
      {loadingRoomEvent && <div>{Localization.GetRoomEvent}...</div>}
      {errorRoomEvent && <div>{Localization.ErrorGetRoomEvent}</div>}
      {errorSendRoomEvent && <div>{Localization.ErrorSendingRoomEvent}</div>}
    </div>
  );
};
