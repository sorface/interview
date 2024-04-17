import { FunctionComponent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { SendMessage } from 'react-use-websocket';
import Peer from 'simple-peer';
import { AuthContext } from '../../../../context/AuthContext';
import { Transcript } from '../../../../types/transcript';
import { VideoChatVideo } from './VideoChatVideo';
import { VideochatParticipant } from './VideochatParticipant';
import { MessagesChat } from './MessagesChat';
import { getAverageVolume } from './utils/getAverageVolume';
import { createAudioAnalyser, frequencyBinCount } from './utils/createAudioAnalyser';
import { limitLength } from './utils/limitLength';
import { randomId } from './utils/randomId';
import { Field } from '../../../../components/FieldsBlock/Field';
import { CodeEditor } from '../CodeEditor/CodeEditor';
import { RoomState } from '../../../../types/room';
import { parseWsMessage } from './utils/parseWsMessage';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { EventsSearchParams, roomsApiDeclaration } from '../../../../apiDeclarations';
import { EventsSearch } from '../../../../types/event';
import { UserType } from '../../../../types/user';
import { useReactionsStatus } from '../../hooks/useReactionsStatus';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { checkIsAudioStream } from './utils/checkIsAudioStream';
import { Canvas } from '@react-three/fiber';
import { AiAssistantExperience } from '../AiAssistant/AiAssistantExperience';

import './VideoChat.css';

const audioVolumeThreshold = 10.0;

const transcriptsMaxLength = 100;

const updateLoudedUserTimeout = 5000;

const viewerOrder = 666;

interface VideoChatProps {
  roomState: RoomState | null;
  viewerMode: boolean;
  lastWsMessage: MessageEvent<any> | null;
  messagesChatEnabled: boolean;
  userVideoStream: MediaStream | null;
  userAudioStream: MediaStream | null;
  videoTrackEnabled: boolean;
  micDisabledAutomatically: React.MutableRefObject<boolean>;
  onSendWsMessage: SendMessage;
  onUpdatePeersLength: (length: number) => void;
  onMuteMic: () => void;
  onUnmuteMic: () => void;
};

interface PeerMeta {
  peerID: string;
  nickname: string;
  avatar: string;
  peer: Peer.Instance;
  targetUserId: string;
  participantType: UserType;
}

const createTranscript = (body: { userNickname: string; value: string; fromChat: boolean; }): Transcript => ({
  frontendId: randomId(),
  ...body,
});

const getChatMessageEvents = (roomEventsSearch: EventsSearch, type: string, toChat: boolean) => {
  const roomEvents = roomEventsSearch[type];
  if (!roomEvents) {
    return [];
  }
  return roomEvents.map(chatMessageEvent => {
    try {
      const chatMessageEventParsed = JSON.parse(chatMessageEvent?.payload);
      return createTranscript({
        fromChat: toChat,
        userNickname: chatMessageEventParsed.Nickname || 'Nickname not found',
        value: chatMessageEventParsed.Message,
      });
    } catch {
      return createTranscript({
        fromChat: toChat,
        userNickname: 'Message not found',
        value: '',
      });
    };
  }).reverse();
};

export const VideoChat: FunctionComponent<VideoChatProps> = ({
  roomState,
  viewerMode,
  lastWsMessage,
  messagesChatEnabled,
  userVideoStream,
  userAudioStream,
  videoTrackEnabled,
  micDisabledAutomatically,
  onSendWsMessage,
  onUpdatePeersLength,
  onMuteMic,
  onUnmuteMic,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const {
    apiMethodState: apiRoomEventsSearchState,
    fetchData: fetchRoomEventsSearch,
  } = useApiMethod<EventsSearch, EventsSearchParams>(roomsApiDeclaration.eventsSearch);
  const {
    data: roomEventsSearch,
  } = apiRoomEventsSearchState;
  const [transcripts, setTranscripts] = useState<Transcript[]>([]);
  const userVideo = useRef<HTMLVideoElement>(null);
  const [peers, setPeers] = useState<PeerMeta[]>([]);
  const peersRef = useRef<PeerMeta[]>([]);
  const userIdToAudioAnalyser = useRef<Record<string, AnalyserNode>>({});
  const requestRef = useRef<number>();
  const louderUserId = useRef(auth?.id || '');
  const [videoOrder, setVideoOrder] = useState<Record<string, number>>({
    [auth?.id || '']: 1,
  });
  const updateLouderUserTimeout = useRef(0);
  const intervieweeFrameRef = useRef<HTMLIFrameElement>(null);
  const { activeReactions } = useReactionsStatus({
    lastMessage: lastWsMessage,
  });

  useEffect(() => {
    if (videoTrackEnabled && userVideoStream) {
      try {
        peers.forEach(peer => {
          const videoTrack = userVideoStream.getVideoTracks()[0];
          peer.peer.addTrack(videoTrack, userVideoStream);
        });
      } catch (e) {
        console.error('add video track error: ', e);
      }
    }
  }, [videoTrackEnabled, peers, userVideoStream]);

  useEffect(() => {
    onUpdatePeersLength(peers.length);
  }, [peers, onUpdatePeersLength]);

  useEffect(() => {
    if (!roomState) {
      return;
    }
    fetchRoomEventsSearch({
      roomId: roomState.id,
    });
  }, [roomState, fetchRoomEventsSearch]);

  useEffect(() => {
    if (!roomEventsSearch) {
      return;
    }
    const newTranscripts = [
      ...getChatMessageEvents(roomEventsSearch, 'ChatMessage', true),
      ...getChatMessageEvents(roomEventsSearch, 'VoiceRecognition', false),
      createTranscript({
        userNickname: localizationCaptions[LocalizationKey.ChatWelcomeMessageNickname],
        value: `${localizationCaptions[LocalizationKey.ChatWelcomeMessage]}, ${auth?.nickname}.`,
        fromChat: true
      }),
    ];
    setTranscripts(newTranscripts);
  }, [roomEventsSearch, auth?.nickname, localizationCaptions]);

  useEffect(() => {
    const frequencyData = new Uint8Array(frequencyBinCount);
    let prevTime = performance.now();
    const updateAudioAnalyser = () => {
      const time = performance.now();
      const delta = time - prevTime;
      let somebodyIsSpeaking = false;
      let newLouderUserId = '';
      let louderVolume = -1;
      const result: Record<string, number> = {};
      for (const [userId, analyser] of Object.entries(userIdToAudioAnalyser.current)) {
        analyser.getByteFrequencyData(frequencyData);
        const averageVolume = getAverageVolume(frequencyData);
        if (averageVolume < audioVolumeThreshold) {
          continue;
        }
        if (auth?.id && (userId !== auth.id)) {
          somebodyIsSpeaking = true;
        }
        if (averageVolume > louderVolume) {
          newLouderUserId = userId;
          louderVolume = averageVolume;
        }
        result[userId] = averageVolume;
      }

      const micEnabled = userAudioStream?.getAudioTracks().some(audioTrack => audioTrack.enabled);

      if (somebodyIsSpeaking && micEnabled) {
        micDisabledAutomatically.current = true;
        onMuteMic();
      } else if (!somebodyIsSpeaking && !micEnabled && micDisabledAutomatically.current) {
        onUnmuteMic();
      }

      if (updateLouderUserTimeout.current > 0) {
        updateLouderUserTimeout.current -= delta;
        prevTime = time;
        requestRef.current = requestAnimationFrame(updateAudioAnalyser);
        return;
      }

      if (newLouderUserId && newLouderUserId !== louderUserId.current) {
        updateLouderUserTimeout.current = updateLoudedUserTimeout;
        setVideoOrder({
          [newLouderUserId]: 1,
          [louderUserId.current]: 2,
        });
        louderUserId.current = newLouderUserId;
      }
      prevTime = time;

      requestRef.current = requestAnimationFrame(updateAudioAnalyser);
    };
    requestRef.current = requestAnimationFrame(updateAudioAnalyser);

    return () => {
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };

  }, [auth, louderUserId, userAudioStream, micDisabledAutomatically, onMuteMic, onUnmuteMic]);

  const createPeer = useCallback((to: string, forViewer?: boolean) => {
    if (viewerMode) {
      onSendWsMessage(JSON.stringify({
        Type: 'sending signal',
        Value: JSON.stringify({
          To: to,
          Signal: 'fake-viewer-signal',
        }),
      }));
      return new Peer();
    }

    const streams: MediaStream[] = [];
    userAudioStream && streams.push(userAudioStream);
    userVideoStream && streams.push(userVideoStream);

    const peer = new Peer({
      initiator: true,
      trickle: false,
      streams,
      ...(forViewer && {
        offerOptions: {
          offerToReceiveAudio: false,
          offerToReceiveVideo: false,
        },
      }),
    });

    peer.on('signal', signal => {
      onSendWsMessage(JSON.stringify({
        Type: 'sending signal',
        Value: JSON.stringify({
          To: to,
          Signal: JSON.stringify(signal),
        }),
      }));
    });

    return peer;
  }, [userAudioStream, userVideoStream, viewerMode, onSendWsMessage]);

  const addPeer = useCallback((incomingSignal: Peer.SignalData, callerID: string) => {
    const streams: MediaStream[] = [];
    userAudioStream && streams.push(userAudioStream);
    userVideoStream && streams.push(userVideoStream);

    const peer = new Peer({
      initiator: false,
      trickle: false,
      streams,
    });

    peer.on('signal', signal => {
      onSendWsMessage(JSON.stringify({
        Type: 'returning signal',
        Value: JSON.stringify({
          To: callerID,
          Signal: JSON.stringify(signal),
        }),
      }));
    });

    peer.signal(incomingSignal);

    return peer;
  }, [userAudioStream, userVideoStream, onSendWsMessage]);

  useEffect(() => {
    return () => {
      if (!userAudioStream) {
        return;
      }
      userAudioStream.getTracks().forEach(track => track.stop());
    };
  }, [userAudioStream]);

  useEffect(() => {
    return () => {
      if (!userVideoStream) {
        return;
      }
      userVideoStream.getTracks().forEach(track => track.stop());
    };
  }, [userVideoStream]);

  useEffect(() => {
    if (!lastWsMessage?.data || !intervieweeFrameRef.current) {
      return;
    }
    let origin = (new URL(intervieweeFrameRef.current.src)).origin;
    intervieweeFrameRef.current.contentWindow?.postMessage(lastWsMessage.data, origin);
  }, [lastWsMessage]);

  useEffect(() => {
    if (!lastWsMessage || !auth) {
      return;
    }
    try {
      const parsedMessage = parseWsMessage(lastWsMessage?.data);
      const parsedPayload = parsedMessage?.Value;
      switch (parsedMessage?.Type) {
        case 'all users':
          if (!Array.isArray(parsedPayload)) {
            break;
          }
          parsedPayload.forEach(userInChat => {
            if (userInChat.Id === auth.id) {
              return;
            }
            const peer = createPeer(userInChat.Id);
            const newPeerMeta = {
              peerID: userInChat.Id,
              nickname: userInChat.Nickname,
              avatar: userInChat.Avatar,
              targetUserId: userInChat.Id,
              participantType: userInChat.ParticipantType,
              peer,
            };

            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });

            peersRef.current.push(newPeerMeta)
          });
          setPeers([...peersRef.current]);
          break;
        case 'user joined':
          const fromUser = parsedPayload.From;
          if (!viewerMode && fromUser.ParticipantType === 'Viewer') {
            const peer = createPeer(fromUser.Id, true);
            const newPeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
            };

            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });

            const peersRefFiltered = peersRef.current.filter(
              peer => peer.peerID !== newPeerMeta.peerID
            );
            peersRef.current = peersRefFiltered;

            peersRef.current.push(newPeerMeta);
            setPeers([...peersRef.current]);
            break;
          }
          if (viewerMode && fromUser.ParticipantType === 'Viewer') {
            const peer = new Peer();
            const newPeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
            };
            const peersRefFiltered = peersRef.current.filter(
              peer => peer.peerID !== newPeerMeta.peerID
            );
            peersRef.current = peersRefFiltered;
            peersRef.current.push(newPeerMeta);
            setPeers([...peersRef.current]);
            break;
          }
          if (viewerMode) {
            const peer = addPeer(JSON.parse(parsedPayload.Signal), fromUser.Id);
            const newPeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
            };
            const peersRefFiltered = peersRef.current.filter(
              peer => peer.peerID !== newPeerMeta.peerID
            );
            peersRef.current = peersRefFiltered;
            peersRef.current.push(newPeerMeta);

            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });
            setPeers([...peersRef.current]);
            break;
          }
          const peer = addPeer(JSON.parse(parsedPayload.Signal), fromUser.Id);
          const newPeerMeta = {
            peerID: fromUser.Id,
            nickname: fromUser.Nickname,
            avatar: fromUser.Avatar,
            targetUserId: fromUser.Id,
            participantType: fromUser.ParticipantType,
            peer,
          };
          const peersRefFiltered = peersRef.current.filter(
            peer => peer.peerID !== newPeerMeta.peerID
          );
          peersRef.current = peersRefFiltered;
          peersRef.current.push(newPeerMeta);

          peer.on('stream', (stream) => {
            const audioStream = checkIsAudioStream(stream);
            if (!audioStream) {
              return;
            }
            userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
          });
          setPeers([...peersRef.current]);
          break;
        case 'user left':
          const leftUserId = parsedPayload.Id;
          const leftUserPeer = peersRef.current.find(p => p.targetUserId === leftUserId);
          if (leftUserPeer) {
            leftUserPeer.peer.destroy();
          }
          const peersAfterLeft = peersRef.current.filter(p => p.targetUserId !== leftUserId);
          peersRef.current = peersAfterLeft;
          setPeers([...peersRef.current]);
          break;
        case 'receiving returned signal':
          const item = peersRef.current.find(p => p.peerID === parsedPayload.From);
          if (item) {
            item.peer.signal(parsedPayload.Signal);
          }
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse ws message error: ', err);
    }
  }, [auth, lastWsMessage, viewerMode, addPeer, createPeer]);

  useEffect(() => {
    if (!lastWsMessage) {
      return;
    }
    try {
      const parsedData = parseWsMessage(lastWsMessage?.data);
      switch (parsedData?.Type) {
        case 'ChatMessage':
          setTranscripts(transcripts => limitLength(
            [
              ...transcripts,
              createTranscript({
                userNickname: parsedData.Value.Nickname,
                value: parsedData.Value.Message,
                fromChat: true,
              }),
            ],
            transcriptsMaxLength
          ));
          break;
        case 'VoiceRecognition':
          setTranscripts(transcripts => limitLength(
            [
              ...transcripts,
              createTranscript({
                userNickname: parsedData.Value.Nickname,
                value: parsedData.Value.Message,
                fromChat: false,
              }),
            ],
            transcriptsMaxLength
          ));
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse chat message error: ', err);
    }
  }, [lastWsMessage]);

  useEffect(() => {
    if (!userAudioStream || !auth?.id) {
      return;
    }
    try {
      userIdToAudioAnalyser.current[auth.id] = createAudioAnalyser(userAudioStream);
    } catch { }
  }, [userAudioStream, auth?.id]);

  useEffect(() => {
    if (!userVideoStream) {
      return;
    }
    if (userVideo.current) {
      userVideo.current.srcObject = userVideoStream;
    }
  }, [userVideoStream]);

  const handleTextMessageSubmit = (message: string) => {
    onSendWsMessage(JSON.stringify({
      Type: 'chat-message',
      Value: message,
    }));
  };

  return (
    <div className='room-columns'>
      <Field className='videochat-field'>
        <div className='videochat'>
          <VideochatParticipant
            order={3}
            viewer={false}
            nickname={localizationCaptions[LocalizationKey.AiAssistantName]}
            videoTrackEnabled={true}
          >
            <div className='videochat-ai-assistant'>
              <Canvas shadows camera={{ position: [0, 0.5, 6.5], fov: 38 }} className='videochat-video'>
                <AiAssistantExperience lastTranscription={transcripts[transcripts.length - 1]} />
              </Canvas>
            </div>
          </VideochatParticipant>
          <VideochatParticipant
            order={viewerMode ? viewerOrder - 1 : videoOrder[auth?.id || '']}
            viewer={viewerMode}
            avatar={auth?.avatar}
            nickname={`${auth?.nickname} (${localizationCaptions[LocalizationKey.You]})`}
            videoTrackEnabled={videoTrackEnabled}
            reaction={activeReactions[auth?.id || '']}
          >
            <video
              ref={userVideo}
              className='videochat-video'
              muted
              autoPlay
              playsInline
            >
              Video not supported
            </video>
          </VideochatParticipant>

          {peers.map(peer => (
            <VideochatParticipant
              key={peer.targetUserId}
              viewer={peer.participantType === 'Viewer'}
              order={peer.participantType === 'Viewer' ? viewerOrder : videoOrder[peer.targetUserId]}
              avatar={peer?.avatar}
              nickname={peer?.nickname}
              reaction={activeReactions[peer.peerID]}
            >
              <VideoChatVideo peer={peer.peer} />
            </VideochatParticipant>
          ))}
        </div>
      </Field>
      <Field className='videochat-field videochat-field-main'>
        <CodeEditor
          roomState={roomState}
          readOnly={viewerMode}
          lastWsMessage={lastWsMessage}
          onSendWsMessage={onSendWsMessage}
        />
      </Field>
      {!!messagesChatEnabled && (
        <Field className='videochat-field videochat-field-chat'>
          <MessagesChat
            transcripts={transcripts}
            onMessageSubmit={handleTextMessageSubmit}
          />
        </Field>
      )}
    </div>
  );
};
