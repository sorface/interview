import { FunctionComponent, useContext, useEffect, useRef, useState } from 'react';
import { AuthContext } from '../../../../context/AuthContext';
import { Transcript } from '../../../../types/transcript';
import { VideoChatVideo } from './VideoChatVideo';
import { VideochatParticipant } from './VideochatParticipant';
import { MessagesChat } from './MessagesChat';
import { limitLength } from './utils/limitLength';
import { randomId } from '../../../../utils/randomId';
import { RoomCodeEditor } from '../RoomCodeEditor/RoomCodeEditor';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { RoomIdParam, roomsApiDeclaration } from '../../../../apiDeclarations';
import { EventsSearch } from '../../../../types/event';
import { useReactionsStatus } from '../../hooks/useReactionsStatus';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
// AiAssistant
// import { Canvas } from '@react-three/fiber';
// import { AiAssistantExperience } from '../AiAssistant/AiAssistantExperience';
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

const transcriptsMaxLength = 100;
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
  setRecognitionEnabled: (enabled: boolean) => void;
  handleInvitationsOpen: () => void;
  handleStartReviewRoom: () => void;
  handleSettingsOpen: () => void;
  handleLeaveRoom: () => void;
};

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

const findUserByOrder = (videoOrder: Record<string, number>) => {
  const lounderUser = Object.entries(videoOrder).find(([userId, order]) => order === 1);
  if (lounderUser) {
    return lounderUser[0];
  }
  return null;
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
    lastWsMessageParsed,
    codeEditorEnabled,
    peers,
    videoOrder,
    peerToStream,
    allUsers,
    sendWsMessage,
  } = useContext(RoomContext);
  const {
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
  const userVideoMainContentBackground = useRef<HTMLVideoElement>(null);
  const screenSharePeer = peers.find(peer => peer.screenShare);
  const [codeEditorInitialValue, setCodeEditorInitialValue] = useState<string | null>(null);
  const { activeReactions } = useReactionsStatus({
    lastWsMessageParsed,
  });

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
    if (codeEditorEnabled) {
      return;
    }
    if (!lastWsMessageParsed || !auth) {
      return;
    }
    try {
      // ScreenShare
      // const screenShare = !!(parsedPayload?.ScreenShare);
      switch (lastWsMessageParsed?.Type) {
        case 'ChangeCodeEditor':
          if (lastWsMessageParsed.Value.Source === 'User') {
            break;
          }
          setCodeEditorInitialValue(lastWsMessageParsed.Value.Content);
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse ws message error: ', err);
    }
  }, [auth, lastWsMessageParsed, viewerMode, codeEditorEnabled]);

  useEffect(() => {
    if (!lastWsMessageParsed) {
      return;
    }
    try {
      switch (lastWsMessageParsed.Type) {
        case 'ChatMessage':
          setTextMessages(transcripts => limitLength(
            [
              ...transcripts,
              {
                id: lastWsMessageParsed.Id,
                userId: lastWsMessageParsed.CreatedById,
                userNickname: lastWsMessageParsed.Value.Nickname,
                value: lastWsMessageParsed.Value.Message,
                createdAt: lastWsMessageParsed.CreatedAt,
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
  }, [lastWsMessageParsed]);

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
    if (userVideoMainContentBackground.current) {
      userVideoMainContentBackground.current.srcObject = userVideoStream;
    }
  }, [userVideoStream, needToRenderMainField]);

  useEffect(() => {
    if (videoOrder[auth?.id || ''] !== 1) {
      return;
    }
    if (userVideoMainContent.current) {
      userVideoMainContent.current.srcObject = userVideoStream;
    }
    if (userVideoMainContentBackground.current) {
      userVideoMainContentBackground.current.srcObject = userVideoStream;
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
        <>
          <video
            ref={userVideoMainContentBackground}
            className='absolute w-full h-full object-cover blur-lg'
            muted
            autoPlay
            playsInline
          >
            Video not supported
          </video>
          <video
            ref={userVideoMainContent}
            className='videochat-video relative'
            muted
            autoPlay
            playsInline
          >
            Video not supported
          </video>

        </>
      );
    }
    const userOrder1Peer = peers.find(peer => peer.targetUserId === userOrder1);
    if (!userOrder1Peer) {
      return <></>;
    }
    return (
      <VideoChatVideo
        blurBg
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
      <div className='videochat-field relative videochat-field-main bg-wrap rounded-1.125'>
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
              className='videochat-video object-cover'
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
                  cover
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
