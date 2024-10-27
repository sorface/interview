import { FunctionComponent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import Peer from 'simple-peer';
import toast from 'react-hot-toast';
import { AuthContext } from '../../../../context/AuthContext';
import { Transcript } from '../../../../types/transcript';
import { VideoChatVideo } from './VideoChatVideo';
import { VideochatParticipant } from './VideochatParticipant';
import { MessagesChat } from './MessagesChat';
import { getAverageVolume } from './utils/getAverageVolume';
import { createAudioAnalyser, frequencyBinCount } from './utils/createAudioAnalyser';
import { limitLength } from './utils/limitLength';
import { randomId } from '../../../../utils/randomId';
import { RoomCodeEditor } from '../RoomCodeEditor/RoomCodeEditor';
import { parseWsMessage } from './utils/parseWsMessage';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { RoomIdParam, roomsApiDeclaration } from '../../../../apiDeclarations';
import { EventsSearch } from '../../../../types/event';
import { User, UserType } from '../../../../types/user';
import { useReactionsStatus } from '../../hooks/useReactionsStatus';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { checkIsAudioStream } from './utils/checkIsAudioStream';
// AiAssistant
// import { Canvas } from '@react-three/fiber';
// import { AiAssistantExperience } from '../AiAssistant/AiAssistantExperience';
import { usePeerStream } from '../../hooks/usePeerStream';
import { RoomToolsPanel } from '../RoomToolsPanel/RoomToolsPanel';
import { UserStreamsContext } from '../../context/UserStreamsContext';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';
import { ContextMenu } from '../../../../components/ContextMenu/ContextMenu';
import { Loader } from '../../../../components/Loader/Loader';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../Icon/Icon';
import { Reactions } from '../Reactions/Reactions';
import { RoomContext } from '../../context/RoomContext';

import './VideoChat.css';

const audioVolumeThreshold = 10.0;

const transcriptsMaxLength = 100;

const updateLoudedUserTimeout = 5000;

const viewerOrder = 666;

interface VideoChatProps {
  messagesChatEnabled: boolean;
  recognitionNotSupported: boolean;
  recognitionEnabled: boolean;
  reactionsVisible: boolean;
  currentUserExpert: boolean;
  loadingRoomStartReview: boolean;
  errorRoomStartReview: string | null;
  // ScreenShare
  // screenStream: MediaStream | null;
  onUpdatePeersLength: (length: number) => void;
  setRecognitionEnabled: (enabled: boolean) => void;
  handleInvitationsOpen: () => void;
  handleStartReviewRoom: () => void;
  handleSettingsOpen: () => void;
  handleLeaveRoom: () => void;
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

const getChatMessageEvents = (roomEventsSearch: EventsSearch, type: string, toChat: boolean) => {
  const roomEvents = roomEventsSearch[type];
  if (!roomEvents) {
    return [];
  }
  return roomEvents.map(chatMessageEvent => {
    try {
      const chatMessageEventParsed = JSON.parse(chatMessageEvent?.payload);
      return {
        id: chatMessageEvent.id,
        userId: chatMessageEvent.createdById,
        userNickname: chatMessageEventParsed.Nickname || 'Nickname not found',
        value: chatMessageEventParsed.Message,
        createdAt: (new Date()).toISOString(),
      };
    } catch {
      return {
        id: randomId(),
        userId: randomId(),
        userNickname: 'Message not found',
        value: '',
        createdAt: (new Date()).toISOString(),
      };
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

const getAllUsers = (data: PeerMeta[], auth: User | null) => {
  const users: Map<User['id'], Pick<User, 'nickname' | 'avatar'>> = new Map();
  data.forEach(peer => {
    users.set(peer.peerID, {
      nickname: peer.nickname,
      avatar: peer.avatar,
    });
  });
  if (auth) {
    users.set(auth.id, {
      nickname: auth.nickname,
      avatar: auth.avatar,
    });
  }
  return users;
};

export const VideoChat: FunctionComponent<VideoChatProps> = ({
  messagesChatEnabled,
  recognitionNotSupported,
  recognitionEnabled,
  currentUserExpert,
  errorRoomStartReview,
  reactionsVisible,
  loadingRoomStartReview,
  // ScreenShare
  // screenStream,
  onUpdatePeersLength,
  setRecognitionEnabled,
  handleInvitationsOpen,
  handleLeaveRoom,
  handleSettingsOpen,
  handleStartReviewRoom,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const {
    viewerMode,
    room,
    roomState,
    lastWsMessage,
    codeEditorEnabled,
    sendWsMessage,
  } = useContext(RoomContext);
  const {
    userAudioStream,
    userVideoStream,
    micEnabled,
    cameraEnabled,
    setMicEnabled,
    setCameraEnabled,
  } = useContext(UserStreamsContext);
  const {
    apiMethodState: apiRoomEventsSearchState,
    fetchData: fetchRoomEventsSearch,
  } = useApiMethod<EventsSearch, RoomIdParam>(roomsApiDeclaration.eventsSearch);
  const {
    data: roomEventsSearch,
  } = apiRoomEventsSearchState;
  // AiAssistant
  // const [transcripts, setTranscripts] = useState<Transcript[]>([]);
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
  const [codeEditorInitialValue, setCodeEditorInitialValue] = useState<string | null>(null);
  const updateLouderUserTimeout = useRef(0);
  const { activeReactions } = useReactionsStatus({
    lastMessage: lastWsMessage,
  });
  const allUsers = getAllUsers(peers, auth);

  const createPeer = useCallback((to: string, forViewer?: boolean, screenShare?: boolean) => {
    if (viewerMode) {
      sendWsMessage(JSON.stringify({
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
    // ScreenShare
    // if (screenShare) {
    //   screenStream && streams.push(screenStream);
    // } else {
    userVideoStream && streams.push(userVideoStream);
    // }

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
      sendWsMessage(JSON.stringify({
        Type: 'sending signal',
        Value: JSON.stringify({
          To: to,
          Signal: JSON.stringify(signal),
          ScreenShare: screenShare,
        }),
      }));
    });

    return peer;
  }, [userAudioStream, userVideoStream, viewerMode, sendWsMessage]);

  // ScreenShare  
  // useEffect(() => {
  //   if (screenStream && auth?.id) {
  //     try {
  //       peersRef.current.forEach(peer => {
  //         if (peer.targetUserId === auth.id) {
  //           return;
  //         }
  //         const newPeer = createPeer(peer.targetUserId, false, true);
  //         const newPeerMeta: PeerMeta = {
  //           peerID: peer.targetUserId,
  //           nickname: peer.nickname,
  //           avatar: peer.avatar,
  //           targetUserId: peer.targetUserId,
  //           participantType: peer.participantType,
  //           peer: newPeer,
  //           screenShare: true,
  //         };

  //         peersRef.current.push(newPeerMeta);
  //         setPeers([...peersRef.current]);
  //       });
  //     } catch (e) {
  //       console.error('add screenStream error: ', e);
  //     }
  //   }
  // }, [auth, screenStream, createPeer]);

  useEffect(() => {
    onUpdatePeersLength(peers.filter(peer => !peer.screenShare).length);
  }, [peers, onUpdatePeersLength]);

  useEffect(() => {
    if (!roomState) {
      return;
    }
    setCodeEditorInitialValue(roomState.codeEditor.content);
    fetchRoomEventsSearch({
      roomId: roomState.id,
    });
  }, [roomState, fetchRoomEventsSearch]);

  useEffect(() => {
    if (!roomEventsSearch) {
      return;
    }
    // AiAssistant
    // const newTranscripts = getChatMessageEvents(roomEventsSearch, 'VoiceRecognition', false);
    const newTextMessages = [
      ...getChatMessageEvents(roomEventsSearch, 'ChatMessage', true),
      {
        id: randomId(),
        userId: randomId(),
        userNickname: localizationCaptions[LocalizationKey.ChatWelcomeMessageNickname],
        value: `${localizationCaptions[LocalizationKey.ChatWelcomeMessage]}, ${auth?.nickname}.`,
        createdAt: (new Date()).toISOString(),
      },
    ];
    setTextMessages(newTextMessages);
    // AiAssistant
    // setTranscripts(newTranscripts);
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
      sendWsMessage(JSON.stringify({
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
  }, [userAudioStream, userVideoStream, sendWsMessage]);

  useEffect(() => {
    if (!lastWsMessage || !auth) {
      return;
    }
    try {
      const parsedMessage = parseWsMessage(lastWsMessage?.data);
      const parsedPayload = parsedMessage?.Value;
      const screenShare = !!(parsedPayload?.ScreenShare);
      switch (parsedMessage?.Type) {
        case 'ChangeCodeEditor':
          if (typeof parsedPayload !== 'string') {
            return;
          }
          setCodeEditorInitialValue(parsedPayload);
          break;
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
      const parsedMessage = parseWsMessage(lastWsMessage?.data);
      const parsedPayload = parsedMessage?.Value;
      switch (parsedMessage?.Type) {
        case 'user joined':
          const fromUser = parsedPayload.From;
          toast.success(
            `${fromUser.Nickname} ${localizationCaptions[LocalizationKey.UserConnectedToRoom]}`
          );
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse ws message error: ', err);
    }
  }, [lastWsMessage, localizationCaptions]);

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
              {
                id: parsedData.Id,
                userId: parsedData.CreatedById,
                userNickname: parsedData.Value.Nickname,
                value: parsedData.Value.Message,
                createdAt: parsedData.CreatedAt,
              },
            ],
            transcriptsMaxLength
          ));
          break;
        // AiAssistant
        // case 'VoiceRecognition':
        //   setTranscripts(transcripts => limitLength(
        //     [
        //       ...transcripts,
        //       createMessage({
        //         userNickname: parsedData.Value.Nickname,
        //         value: parsedData.Value.Message,
        //         createdAt: parsedData.CreatedAt,
        //       }),
        //     ],
        //     transcriptsMaxLength
        //   ));
        //   break;
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
  }, [auth?.id, videoOrder, userVideoStream]);

  useEffect(() => {
    if (viewerMode) {
      return;
    }
    setRecognitionEnabled(micEnabled);
  }, [viewerMode, micEnabled, setRecognitionEnabled]);

  const handleTextMessageSubmit = (message: string) => {
    sendWsMessage(JSON.stringify({
      Type: 'chat-message',
      Value: message,
    }));
  };

  const handleMicSwitch = () => {
    setMicEnabled(!micEnabled);
  };

  const handleCameraSwitch = () => {
    setCameraEnabled(!cameraEnabled);
  };

  const handleVoiceRecognitionSwitch = () => {
    setRecognitionEnabled(!recognitionEnabled);
  };

  const handleCodeEditorSwitch = () => {
    sendWsMessage(JSON.stringify({
      Type: 'room-code-editor-enabled',
      Value: JSON.stringify({ Enabled: !codeEditorEnabled }),
    }));
  };

  const renderMain = () => {
    if (codeEditorEnabled) {
      return (
        <RoomCodeEditor
          initialValue={codeEditorInitialValue}
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
      <RoomToolsPanel.Wrapper rightPos='21.5rem' bottomPos='1.5rem'>
        {!viewerMode && (
          <RoomToolsPanel.ButtonsGroupWrapper>
            <RoomToolsPanel.SwitchButton
              enabled={micEnabled}
              iconEnabledName={IconNames.MicOn}
              iconDisabledName={IconNames.MicOff}
              onClick={handleMicSwitch}
            />
            <Gap sizeRem={0.125} />
            <RoomToolsPanel.SwitchButton
              enabled={cameraEnabled}
              iconEnabledName={IconNames.VideocamOn}
              iconDisabledName={IconNames.VideocamOff}
              onClick={handleCameraSwitch}
            />
            {!recognitionNotSupported && (
              <>
                <Gap sizeRem={0.125} />
                <RoomToolsPanel.SwitchButton
                  enabled={recognitionEnabled}
                  htmlDisabled={!micEnabled}
                  iconEnabledName={IconNames.RecognitionOn}
                  iconDisabledName={IconNames.RecognitionOff}
                  onClick={handleVoiceRecognitionSwitch}
                />
              </>
            )}
          </RoomToolsPanel.ButtonsGroupWrapper>
        )}
        {reactionsVisible && (
          <RoomToolsPanel.ButtonsGroupWrapper>
            <Reactions
              room={room}
            />
            {!viewerMode && (
              <>
                <Gap sizeRem={0.125} />
                <RoomToolsPanel.SwitchButton
                  enabled={true}
                  iconEnabledName={IconNames.CodeEditor}
                  iconDisabledName={IconNames.CodeEditor}
                  onClick={handleCodeEditorSwitch}
                />
              </>
            )}
          </RoomToolsPanel.ButtonsGroupWrapper>
        )}
        {!viewerMode && (
          <RoomToolsPanel.ButtonsGroupWrapper>
            {/* ScreenShare */}
            {/* <RoomToolsPanel.SwitchButton
              enabled={true}
              iconEnabledName={IconNames.TV}
              iconDisabledName={IconNames.TV}
              onClick={handleScreenShare}
            /> */}
            {currentUserExpert && (
              <>
                <Gap sizeRem={0.125} />
                <RoomToolsPanel.SwitchButton
                  enabled={true}
                  iconEnabledName={IconNames.PersonAdd}
                  iconDisabledName={IconNames.PersonAdd}
                  onClick={handleInvitationsOpen}
                />
              </>
            )}
            <Gap sizeRem={0.125} />
            <RoomToolsPanel.SwitchButton
              enabled={true}
              iconEnabledName={IconNames.Settings}
              iconDisabledName={IconNames.Settings}
              onClick={handleSettingsOpen}
            />
          </RoomToolsPanel.ButtonsGroupWrapper>
        )}
        <RoomToolsPanel.ButtonsGroupWrapper noPaddingBottom>
          <ContextMenu
            toggleContent={
              <RoomToolsPanel.SwitchButton
                enabled={true}
                iconEnabledName={IconNames.Call}
                iconDisabledName={IconNames.Call}
                onClick={currentUserExpert ? () => { } : handleLeaveRoom}
                danger
              />
            }
            position='left'
          >
            {loadingRoomStartReview && (
              <Loader />
            )}
            {errorRoomStartReview && (
              <div className='flex items-center justify-center'>
                <Typography size='m' error>
                  <Icon name={IconNames.Information} />
                </Typography>
                <Typography size='m' error>
                  {errorRoomStartReview}
                </Typography>
              </div>
            )}
            {currentUserExpert && (
              <ContextMenu.Item title={localizationCaptions[LocalizationKey.CompleteAndEvaluateCandidate]} onClick={handleStartReviewRoom} />
            )}
            <ContextMenu.Item title={localizationCaptions[LocalizationKey.Exit]} onClick={handleLeaveRoom} />
          </ContextMenu>
        </RoomToolsPanel.ButtonsGroupWrapper>
      </RoomToolsPanel.Wrapper>
      <div className='videochat-field videochat-field-main bg-wrap rounded-1.125'>
        {renderMain()}
      </div>

      <div className='relative videochat-field bg-wrap rounded-1.125'>
        <div className={`videochat ${messagesChatEnabled ? 'invisible h-full' : 'visible'}`}>
          {/* AiAssistant */}
          {/* <VideochatParticipant
            order={3}
            viewer={false}
            nickname={localizationCaptions[LocalizationKey.AiAssistantName]}
          >
            <div className='videochat-ai-assistant'>
              <Canvas shadows camera={{ position: [0, 0.5, 6.5], fov: 38 }} className='videochat-video'>
                <AiAssistantExperience lastTranscription={transcripts[transcripts.length - 1]} />
              </Canvas>
            </div>
          </VideochatParticipant> */}
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
            allUsers={allUsers}
            onMessageSubmit={handleTextMessageSubmit}
          />
        </div>
      </div>
    </>
  );
};
