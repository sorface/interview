import { useEffect, useState } from 'react';
import { RoomState, RoomStateAdditionalStatefulPayload } from '../../../types/room';

export type EventsState = Record<string, boolean>;

interface UseEventsStateParams {
  roomState: RoomState | null;
  lastWsMessage: MessageEvent<any> | null;
}

export const useEventsState = ({
  lastWsMessage,
  roomState,
}: UseEventsStateParams) => {
  const [eventsState, setEventsState] = useState<EventsState>({});

  useEffect(() => {
    if (!roomState) {
      return;
    }
    const parsedStates: EventsState = {};
    roomState.states.forEach(roomState =>
      parsedStates[roomState.type] = (JSON.parse(roomState.payload) as RoomStateAdditionalStatefulPayload).enabled
    );
    setEventsState(parsedStates);
  }, [roomState]);

  useEffect(() => {
    if (!lastWsMessage || !eventsState) {
      return;
    }
    try {
      const parsedData = JSON.parse(lastWsMessage?.data);
      if (!parsedData?.Stateful) {
        return;
      }
      const stateType = parsedData.Type;
      const stateValue = (parsedData.Value.AdditionalData as RoomStateAdditionalStatefulPayload).enabled;
      const oldValue = eventsState[stateType];
      if (oldValue !== stateValue) {
        setEventsState({
          ...eventsState,
          [stateType]: stateValue,
        });
      }
    } catch {
    }
  }, [lastWsMessage, eventsState]);

  return eventsState;
};
