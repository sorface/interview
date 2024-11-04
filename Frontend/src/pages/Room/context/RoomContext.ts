import { SendMessage } from 'react-use-websocket';
import { Room, RoomParticipant, RoomState } from '../../../types/room';
import { createContext } from 'react';
import { CodeEditorLang } from '../../../types/question';
import { ParsedWsMessage } from '../utils/parseWsMessage';
import { PeerMeta } from '../components/VideoChat/hoks/useVideoChat';
import { UsePeerStreamState } from '../hooks/usePeerStream';
import { User } from '../../../types/user';

export interface RoomContextType {
  room: Room | null;
  roomState: RoomState | null;
  roomParticipant: RoomParticipant | null;
  viewerMode: boolean;
  lastWsMessageParsed: ParsedWsMessage | null;
  codeEditorEnabled: boolean;
  codeEditorLanguage: CodeEditorLang;
  peers: PeerMeta[];
  videoOrder: Record<string, number>;
  peerToStream: UsePeerStreamState;
  allUsers: Map<string, Pick<User, "nickname" | "avatar">>;
  sendWsMessage: SendMessage;
  setCodeEditorEnabled: (enabled: boolean) => void;
}

const noop = () => { };

const defaultValue: RoomContextType = {
  room: null,
  roomState: null,
  roomParticipant: null,
  viewerMode: true,
  lastWsMessageParsed: null,
  codeEditorEnabled: false,
  codeEditorLanguage: CodeEditorLang.Plaintext,
  peers: [],
  videoOrder: {},
  peerToStream: new Map(),
  allUsers: new Map(),
  sendWsMessage: noop,
  setCodeEditorEnabled: noop,
};

export const RoomContext = createContext<RoomContextType>(defaultValue);
