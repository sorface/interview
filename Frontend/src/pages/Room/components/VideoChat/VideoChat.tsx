import { FunctionComponent, ReactElement, useCallback, useContext, useEffect, useRef, useState } from 'react';
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
import { RoomCodeEditor } from '../RoomCodeEditor/RoomCodeEditor';
import { RoomState } from '../../../../types/room';
import { parseWsMessage } from './utils/parseWsMessage';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { RoomIdParam, roomsApiDeclaration } from '../../../../apiDeclarations';
import { EventsSearch } from '../../../../types/event';
import { UserType } from '../../../../types/user';
import { useReactionsStatus } from '../../hooks/useReactionsStatus';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { checkIsAudioStream } from './utils/checkIsAudioStream';
import { Canvas } from '@react-three/fiber';
import { AiAssistantExperience } from '../AiAssistant/AiAssistantExperience';
import { CodeEditorLang } from '../../../../types/question';
import { usePeerStream } from '../../hooks/usePeerStream';

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
  codeEditorEnabled: boolean;
  codeEditorLanguage: CodeEditorLang;
  userVideoStream: MediaStream | null;
  userAudioStream: MediaStream | null;
  screenStream: MediaStream | null;
  onSendWsMessage: SendMessage;
  onUpdatePeersLength: (length: number) => void;
  renderToolsPanel: () => ReactElement;
};

interface PeerMeta {
  peerID: string;
  nickname: string;
  avatar: string;
  peer: Peer.Instance;
  targetUserId: string;
  participantType: UserType;
  screenShare: boolean;
}

const createMessage = (body: { userNickname: string; value: string; createdAt: string; }): Transcript => ({
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
      return createMessage({
        userNickname: chatMessageEventParsed.Nickname || 'Nickname not found',
        value: chatMessageEventParsed.Message,
        createdAt: (new Date()).toISOString(),
      });
    } catch {
      return createMessage({
        userNickname: 'Message not found',
        value: '',
        createdAt: (new Date()).toISOString(),
      });
    };
  }).reverse();
};

const removeDuplicates = (peersRef: React.MutableRefObject<PeerMeta[]>, newPeerMeta: PeerMeta) =>
  peersRef.current.filter(peer =>
    peer.peerID !== newPeerMeta.peerID ? true : peer.screenShare !== newPeerMeta.screenShare
  );

const findUserByOrder = (videoOrder: Record<string, number>) => {
  const lounderUser = Object.entries(videoOrder).find(([userId, order]) => order === 1);
  if (lounderUser) {
    return lounderUser[0];
  }
  return null;
};

export const VideoChat: FunctionComponent<VideoChatProps> = ({
  roomState,
  viewerMode,
  lastWsMessage,
  messagesChatEnabled,
  codeEditorEnabled,
  codeEditorLanguage,
  userVideoStream,
  userAudioStream,
  screenStream,
  onSendWsMessage,
  onUpdatePeersLength,
  renderToolsPanel,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const {
    apiMethodState: apiRoomEventsSearchState,
    fetchData: fetchRoomEventsSearch,
  } = useApiMethod<EventsSearch, RoomIdParam>(roomsApiDeclaration.eventsSearch);
  const {
    data: roomEventsSearch,
  } = apiRoomEventsSearchState;
  const [transcripts, setTranscripts] = useState<Transcript[]>([]);
  const [textMessages, setTextMessages] = useState<Transcript[]>([]);
  const userVideo = useRef<HTMLVideoElement>(null);
  const userVideoMainContent = useRef<HTMLVideoElement>(null);
  const [peers, setPeers] = useState<PeerMeta[]>([]);
  const { peerToStream, addPeerStream, removePeerStream } = usePeerStream();
  const screenSharePeer = peers.find(peer => peer.screenShare);
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

  const createPeer = useCallback((to: string, forViewer?: boolean, screenShare?: boolean) => {
    if (viewerMode) {
      onSendWsMessage(JSON.stringify({
        Type: 'sending signal',
        Value: JSON.stringify({
          To: to,
          Signal: 'fake-viewer-signal',
          ScreenShare: false,
        }),
      }));
      return new Peer();
    }

    const streams: MediaStream[] = [];
    userAudioStream && streams.push(userAudioStream);
    if (screenShare) {
      screenStream && streams.push(screenStream);
    } else {
      userVideoStream && streams.push(userVideoStream);
    }

    const peer = new Peer({
      initiator: true,
      trickle: false,
      streams,
      ...((forViewer || screenShare) && {
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
          ScreenShare: screenShare,
        }),
      }));
    });

    return peer;
  }, [userAudioStream, userVideoStream, screenStream, viewerMode, onSendWsMessage]);

  useEffect(() => {
    if (screenStream && auth?.id) {
      try {
        peersRef.current.forEach(peer => {
          if (peer.targetUserId === auth.id) {
            return;
          }
          const newPeer = createPeer(peer.targetUserId, false, true);
          const newPeerMeta: PeerMeta = {
            peerID: peer.targetUserId,
            nickname: peer.nickname,
            avatar: peer.avatar,
            targetUserId: peer.targetUserId,
            participantType: peer.participantType,
            peer: newPeer,
            screenShare: true,
          };

          peersRef.current.push(newPeerMeta);
          setPeers([...peersRef.current]);
        });
      } catch (e) {
        console.error('add screenStream error: ', e);
      }
    }
  }, [auth, screenStream, createPeer]);

  useEffect(() => {
    onUpdatePeersLength(peers.filter(peer => !peer.screenShare).length);
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
    const newTranscripts = getChatMessageEvents(roomEventsSearch, 'VoiceRecognition', false);
    const newTextMessages = [
      ...getChatMessageEvents(roomEventsSearch, 'ChatMessage', true),
      createMessage({
        userNickname: localizationCaptions[LocalizationKey.ChatWelcomeMessageNickname],
        value: `${localizationCaptions[LocalizationKey.ChatWelcomeMessage]}, ${auth?.nickname}.`,
        createdAt: (new Date()).toISOString(),
      }),
    ];
    setTextMessages(newTextMessages);
    setTranscripts(newTranscripts);
  }, [roomEventsSearch, auth?.nickname, localizationCaptions]);

  useEffect(() => {
    const frequencyData = new Uint8Array(frequencyBinCount);
    let prevTime = performance.now();
    const updateAudioAnalyser = () => {
      const time = performance.now();
      const delta = time - prevTime;
      let newLouderUserId = '';
      let louderVolume = -1;
      const result: Record<string, number> = {};
      for (const [userId, analyser] of Object.entries(userIdToAudioAnalyser.current)) {
        analyser.getByteFrequencyData(frequencyData);
        const averageVolume = getAverageVolume(frequencyData);
        if (averageVolume < audioVolumeThreshold) {
          continue;
        }
        if (averageVolume > louderVolume) {
          newLouderUserId = userId;
          louderVolume = averageVolume;
        }
        result[userId] = averageVolume;
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

  }, [auth, louderUserId]);

  const addPeer = useCallback((incomingSignal: Peer.SignalData, callerID: string, screenShare?: boolean) => {
    const streams: MediaStream[] = [];
    if (!screenShare) {
      userAudioStream && streams.push(userAudioStream);
      userVideoStream && streams.push(userVideoStream);
    }

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
          ScreenShare: screenShare,
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
      const screenShare = !!(parsedPayload?.ScreenShare);
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
            const newPeerMeta: PeerMeta = {
              peerID: userInChat.Id,
              nickname: userInChat.Nickname,
              avatar: userInChat.Avatar,
              targetUserId: userInChat.Id,
              participantType: userInChat.ParticipantType,
              peer,
              screenShare: false,
            };

            addPeerStream(newPeerMeta.peerID, peer, false);
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
            const newPeerMeta: PeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
              screenShare: false,
            };

            addPeerStream(newPeerMeta.peerID, peer, false);
            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });

            peersRef.current = removeDuplicates(peersRef, newPeerMeta);

            peersRef.current.push(newPeerMeta);
            setPeers([...peersRef.current]);
            break;
          }
          if (viewerMode && fromUser.ParticipantType === 'Viewer') {
            const peer = new Peer();
            const newPeerMeta: PeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
              screenShare: false,
            };
            peersRef.current = removeDuplicates(peersRef, newPeerMeta);
            peersRef.current.push(newPeerMeta);
            setPeers([...peersRef.current]);
            break;
          }
          if (viewerMode || screenShare) {
            const peer = addPeer(JSON.parse(parsedPayload.Signal), fromUser.Id, screenShare);
            addPeerStream(fromUser.Id, peer, true);
            const newPeerMeta: PeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
              screenShare,
            };
            peersRef.current = removeDuplicates(peersRef, newPeerMeta);
            peersRef.current.push(newPeerMeta);

            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream || screenShare) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });
            setPeers([...peersRef.current]);
            break;
          }
          const peer = addPeer(JSON.parse(parsedPayload.Signal), fromUser.Id);
          addPeerStream(fromUser.Id, peer, true);
          const newPeerMeta: PeerMeta = {
            peerID: fromUser.Id,
            nickname: fromUser.Nickname,
            avatar: fromUser.Avatar,
            targetUserId: fromUser.Id,
            participantType: fromUser.ParticipantType,
            peer,
            screenShare,
          };
          peersRef.current = removeDuplicates(peersRef, newPeerMeta);
          peersRef.current.push(newPeerMeta);

          peer.on('stream', (stream) => {
            const audioStream = checkIsAudioStream(stream);
            if (!audioStream || screenShare) {
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
            removePeerStream(leftUserPeer.peerID);
            leftUserPeer.peer.destroy();
          }
          const peersAfterLeft = peersRef.current.filter(p => p.targetUserId !== leftUserId);
          peersRef.current = peersAfterLeft;
          setPeers([...peersRef.current]);
          setVideoOrder({
            [auth.id]: 1,
            [louderUserId.current]: 2,
          });
          break;
        case 'receiving returned signal':
          const item = peersRef.current.find(p =>
            p.peerID === parsedPayload.From && (screenShare ? p.screenShare : true)
          );
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
  }, [auth, lastWsMessage, viewerMode, addPeer, createPeer, addPeerStream, removePeerStream]);

  useEffect(() => {
    if (!lastWsMessage) {
      return;
    }
    try {
      const parsedData = parseWsMessage(lastWsMessage?.data);
      switch (parsedData?.Type) {
        case 'ChatMessage':
          setTextMessages(transcripts => limitLength(
            [
              ...transcripts,
              createMessage({
                userNickname: parsedData.Value.Nickname,
                value: parsedData.Value.Message,
                createdAt: parsedData.CreatedAt,
              }),
            ],
            transcriptsMaxLength
          ));
          break;
        case 'VoiceRecognition':
          setTranscripts(transcripts => limitLength(
            [
              ...transcripts,
              createMessage({
                userNickname: parsedData.Value.Nickname,
                value: parsedData.Value.Message,
                createdAt: parsedData.CreatedAt,
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

  const needToRenderMainField = screenSharePeer || codeEditorEnabled;

  useEffect(() => {
    if (!userVideoStream) {
      return;
    }
    if (userVideo.current) {
      userVideo.current.srcObject = userVideoStream;
    }
    if (userVideoMainContent.current) {
      userVideoMainContent.current.srcObject = userVideoStream;
    }
  }, [userVideoStream, needToRenderMainField]);

  useEffect(() => {
    if (videoOrder[auth?.id || ''] === 1 && userVideoMainContent.current) {
      userVideoMainContent.current.srcObject = userVideoStream;
    }
  }, [auth?.id, videoOrder, userVideoStream])

  const handleTextMessageSubmit = (message: string) => {
    onSendWsMessage(JSON.stringify({
      Type: 'chat-message',
      Value: message,
    }));
  };

  const renderMain = () => {
    if (codeEditorEnabled) {
      return (
        <RoomCodeEditor
          language={codeEditorLanguage}
          roomState={roomState}
          readOnly={viewerMode}
          lastWsMessage={lastWsMessage}
          onSendWsMessage={onSendWsMessage}
        />
      );
    }
    const userOrder1 = findUserByOrder(videoOrder);
    if (!userOrder1 || userOrder1 === auth?.id) {
      return (
        <video
          ref={userVideoMainContent}
          className='videochat-video'
          muted
          autoPlay
          playsInline
        >
          Video not supported
        </video>
      );
    }
    const userOrder1Peer = peers.find(peer => peer.targetUserId === userOrder1);
    if (!userOrder1Peer) {
      return <></>;
    }
    return (
      <VideoChatVideo
        videoStream={peerToStream.get(userOrder1Peer.peerID)?.video}
      />
    );
  };

  return (
    <>
      {renderToolsPanel()}
      <div className='videochat-field videochat-field-main bg-wrap rounded-1.125'>
        {renderMain()}
      </div>

      <div className='relative videochat-field bg-wrap rounded-1.125'>
        <div className={`videochat ${messagesChatEnabled ? 'invisible h-full' : 'visible'}`}>
          <VideochatParticipant
            order={3}
            viewer={false}
            nickname={localizationCaptions[LocalizationKey.AiAssistantName]}
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
          {peers
            .filter(peer => !peer.screenShare)
            .map(peer => (
              <VideochatParticipant
                key={peer.peerID}
                viewer={peer.participantType === 'Viewer'}
                order={peer.participantType === 'Viewer' ? viewerOrder : videoOrder[peer.targetUserId]}
                avatar={peer?.avatar}
                nickname={peer?.nickname}
                reaction={activeReactions[peer.peerID]}
              >
                <VideoChatVideo
                  loaded={peerToStream.get(peer.peerID)?.loaded}
                  audioStream={peerToStream.get(peer.peerID)?.audio}
                  videoStream={peerToStream.get(peer.peerID)?.video}
                />
              </VideochatParticipant>
            ))}
        </div>

        <div className={`absolute top-0 h-full bg-wrap w-full ${messagesChatEnabled ? 'visible' : 'invisible'} z-1`}>
          <MessagesChat
            textMessages={textMessages}
            onMessageSubmit={handleTextMessageSubmit}
          />
        </div>
      </div>
    </>
  );
};
