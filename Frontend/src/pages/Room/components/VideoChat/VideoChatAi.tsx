import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
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
import { RoomToolsPanel } from '../RoomToolsPanel/RoomToolsPanel';
import { UserStreamsContext } from '../../context/UserStreamsContext';
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';
import { ContextMenu } from '../../../../components/ContextMenu/ContextMenu';
import { Loader } from '../../../../components/Loader/Loader';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../Icon/Icon';
import { RoomContext } from '../../context/RoomContext';
import { RoomQuestionPanelAi } from '../RoomQuestionPanelAi/RoomQuestionPanelAi';
import { RoomQuestion } from '../../../../types/room';
import { sortRoomQuestion } from '../../../../utils/sortRoomQestions';

import './VideoChatAi.css';

const transcriptsMaxLength = 100;
const viewerOrder = 666;

interface VideoChatAiProps {
  messagesChatEnabled: boolean;
  recognitionNotSupported: boolean;
  currentUserExpert: boolean;
  loadingRoomStartReview: boolean;
  errorRoomStartReview: string | null;
  // ScreenShare
  // screenStream: MediaStream | null;
  roomQuestionsLoading: boolean;
  roomQuestions: RoomQuestion[];
  initialQuestion?: RoomQuestion;
  handleStartReviewRoom: () => void;
  handleSettingsOpen: () => void;
  handleLeaveRoom: () => void;
}

const getChatMessageEvents = (
  roomEventsSearch: EventsSearch,
  type: string,
) => {
  const roomEvents = roomEventsSearch[type];
  if (!roomEvents) {
    return [];
  }
  return roomEvents
    .map((chatMessageEvent) => {
      try {
        const chatMessageEventParsed = JSON.parse(chatMessageEvent?.payload);
        return {
          id: chatMessageEvent.id,
          userId: chatMessageEvent.createdById,
          userNickname: chatMessageEventParsed.Nickname || 'Nickname not found',
          value: chatMessageEventParsed.Message,
          createdAt: new Date().toISOString(),
        };
      } catch {
        return {
          id: randomId(),
          userId: randomId(),
          userNickname: 'Message not found',
          value: '',
          createdAt: new Date().toISOString(),
        };
      }
    })
    .reverse();
};

export const VideoChatAi: FunctionComponent<VideoChatAiProps> = ({
  messagesChatEnabled,
  recognitionNotSupported,
  currentUserExpert,
  errorRoomStartReview,
  loadingRoomStartReview,
  // ScreenShare
  // screenStream,
  roomQuestionsLoading,
  roomQuestions,
  initialQuestion,
  handleLeaveRoom,
  handleSettingsOpen,
  handleStartReviewRoom,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const {
    viewerMode,
    roomState,
    lastWsMessageParsed,
    codeEditorEnabled,
    peers,
    videoOrder,
    peerToStream,
    allUsers,
    recognitionEnabled,
    sendWsMessage,
    setRecognitionEnabled,
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
  const { data: roomEventsSearch } = apiRoomEventsSearchState;
  const [textMessages, setTextMessages] = useState<Transcript[]>([]);
  const userVideo = useRef<HTMLVideoElement>(null);
  const userVideoMainContent = useRef<HTMLVideoElement>(null);
  const userVideoMainContentBackground = useRef<HTMLVideoElement>(null);
  const screenSharePeer = peers.find((peer) => peer.screenShare);
  const { activeReactions } = useReactionsStatus({
    lastWsMessageParsed,
  });

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
    const newTextMessages = [
      ...getChatMessageEvents(roomEventsSearch, 'ChatMessage', true),
      {
        id: randomId(),
        userId: randomId(),
        userNickname:
          localizationCaptions[LocalizationKey.ChatWelcomeMessageNickname],
        value: `${localizationCaptions[LocalizationKey.ChatWelcomeMessage]}, ${auth?.nickname}.`,
        createdAt: new Date().toISOString(),
      },
    ];
    setTextMessages(newTextMessages);
  }, [roomEventsSearch, auth?.nickname, localizationCaptions]);

  useEffect(() => {
    if (!lastWsMessageParsed) {
      return;
    }
    try {
      switch (lastWsMessageParsed.Type) {
        case 'ChatMessage':
          setTextMessages((transcripts) =>
            limitLength(
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
              transcriptsMaxLength,
            ),
          );
          break;
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
    sendWsMessage(
      JSON.stringify({
        Type: 'chat-message',
        Value: message,
      }),
    );
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

  const renderMain = () => {
    return (
      <>
        <Gap sizeRem={1.5 * 2.5} />
        <RoomQuestionPanelAi
          roomQuestionsLoading={roomQuestionsLoading}
          roomQuestions={
            roomQuestions?.sort(sortRoomQuestion) || []
          }
          initialQuestion={initialQuestion}
        />
        <video
          ref={userVideoMainContent}
          className="w-12.5 h-10 absolute videochat-video object-cover z-1"
          style={{
            right: '5.5rem',
            bottom: '1.5rem',
          }}
          muted
          autoPlay
          playsInline
        >
          Video not supported
        </video>
        <RoomToolsPanel.Wrapper rightPos="1.5rem" bottomPos="1.5rem">
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
          {!viewerMode && (
            <RoomToolsPanel.ButtonsGroupWrapper>
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
              position="left"
            >
              {loadingRoomStartReview && <Loader />}
              {errorRoomStartReview && (
                <div className="flex items-center justify-center">
                  <Typography size="m" error>
                    <Icon name={IconNames.Information} />
                  </Typography>
                  <Typography size="m" error>
                    {errorRoomStartReview}
                  </Typography>
                </div>
              )}
              {currentUserExpert && (
                <ContextMenu.Item
                  title={
                    localizationCaptions[
                    LocalizationKey.CompleteAndEvaluateCandidate
                    ]
                  }
                  onClick={handleStartReviewRoom}
                />
              )}
              <ContextMenu.Item
                title={localizationCaptions[LocalizationKey.Exit]}
                onClick={handleLeaveRoom}
              />
            </ContextMenu>
          </RoomToolsPanel.ButtonsGroupWrapper>
        </RoomToolsPanel.Wrapper>
      </>
    );
  };

  return (
    <>
      <div className='w-full flex justify-center'>
        <div style={{ maxWidth: '840px' }} className="w-full flex flex-col videochat-field relative videochat-field-main rounded-1.125">
          <RoomCodeEditor
            visible={codeEditorEnabled}
          />
          {!codeEditorEnabled && renderMain()}
        </div>
      </div>

      <div className="relative videochat-field bg-wrap rounded-1.125">
        <div
          className={`videochat ${messagesChatEnabled ? 'invisible h-full' : 'visible'}`}
        >
          {peers
            .filter((peer) => !peer.screenShare)
            .map((peer) => (
              <VideochatParticipant
                key={peer.peerID}
                viewer={peer.participantType === 'Viewer'}
                order={
                  peer.participantType === 'Viewer'
                    ? viewerOrder
                    : videoOrder[peer.targetUserId]
                }
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

        <div
          className={`absolute top-0 h-full bg-wrap w-full ${messagesChatEnabled ? 'visible' : 'invisible'} z-1`}
        >
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
