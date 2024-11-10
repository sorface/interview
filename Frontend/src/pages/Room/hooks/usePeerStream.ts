import { useCallback, useReducer } from 'react';
import Peer from 'simple-peer';

export type UsePeerStreamState = Map<string, { audio?: MediaStream; video?: MediaStream; loaded?: boolean }>;

const initialState: UsePeerStreamState = new Map();

type UsePeerStreamAction = {
  name: 'addPeerAudioStream';
  payload: { peerId: string; stream: MediaStream };
} | {
  name: 'addPeerVideoStream';
  payload: { peerId: string; stream: MediaStream };
} | {
  name: 'removePeer';
  payload: { peerId: string; };
} | {
  name: 'setPeerLoaded';
  payload: { peerId: string; loaded: boolean; };
}

const usePeerStreamReducer = (state: UsePeerStreamState, action: UsePeerStreamAction): UsePeerStreamState => {
  switch (action.name) {
    case 'addPeerAudioStream':
      const newStateAddAudioPeer = new Map(state);
      newStateAddAudioPeer.set(
        action.payload.peerId,
        { ...state.get(action.payload.peerId), audio: action.payload.stream },
      );
      return newStateAddAudioPeer;
    case 'addPeerVideoStream':
      const newStateAddVideoPeer = new Map(state);
      newStateAddVideoPeer.set(
        action.payload.peerId,
        { ...state.get(action.payload.peerId), video: action.payload.stream },
      );
      return newStateAddVideoPeer;
    case 'removePeer':
      const newStateRemovePeer = new Map(state);
      newStateRemovePeer.delete(action.payload.peerId);
      return newStateRemovePeer;
    case 'setPeerLoaded':
      const newStateLoadedPeer = new Map(state);
      newStateLoadedPeer.set(
        action.payload.peerId,
        { ...state.get(action.payload.peerId), loaded: action.payload.loaded },
      );
      return newStateLoadedPeer;
    default:
      return state;
  }
};

export const usePeerStream = () => {
  const [peerToStream, dispatch] = useReducer(usePeerStreamReducer, initialState);

  const addPeerStream = useCallback((peerId: string, peer: Peer.Instance, trackLoading: boolean) => {
    dispatch({ name: 'setPeerLoaded', payload: { peerId, loaded: !trackLoading } });
    peer.on('stream', (stream) => {
      const audioStream = !!stream.getAudioTracks().length;
      if (audioStream) {
        dispatch({ name: 'addPeerAudioStream', payload: { peerId, stream } });
        return;
      }
      const videoStream = !!stream.getVideoTracks().length;
      if (videoStream) {
        dispatch({ name: 'addPeerVideoStream', payload: { peerId, stream } });
        return;
      }
    });
    if (trackLoading) {
      peer.on('signal', () => {
        dispatch({ name: 'setPeerLoaded', payload: { peerId, loaded: true } });
      });
    }
  }, []);

  const removePeerStream = useCallback((peerId: string) => {
    dispatch({ name: 'removePeer', payload: { peerId } });
  }, []);

  return {
    peerToStream,
    addPeerStream,
    removePeerStream,
  };
};
