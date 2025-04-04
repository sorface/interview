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
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { RoomIdParam, roomsApiDeclaration } from '../../../../apiDeclarations';
import { EventsSearch } from '../../../../types/event';
import { useReactionsStatus } from '../../hooks/useReactionsStatus';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
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
import { Theme, ThemeContext } from '../../../../context/ThemeContext';
import { Button } from '../../../../components/Button/Button';

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
  handleInvitationsOpen: () => void;
  handleStartReviewRoom: () => void;
  handleSettingsOpen: () => void;
  handleLeaveRoom: () => void;
}

const getChatMessageEvents = (roomEventsSearch: EventsSearch, type: string) => {
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
  currentUserExpert,
  errorRoomStartReview,
  loadingRoomStartReview,
  // ScreenShare
  // screenStream,
  roomQuestionsLoading,
  roomQuestions,
  initialQuestion,
  handleInvitationsOpen,
  handleLeaveRoom,
  handleSettingsOpen,
  handleStartReviewRoom,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const { themeInUi } = useContext(ThemeContext);
  const {
    viewerMode,
    roomState,
    lastWsMessageParsed,
    codeEditorEnabled,
    peers,
    videoOrder,
    peerToStream,
    allUsers,
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
      ...getChatMessageEvents(roomEventsSearch, 'ChatMessage'),
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

  return (
    <>
      <div className="flex-1 flex justify-center">
        <div
          style={{ maxWidth: '840px' }}
          className="w-full flex flex-col relative rounded-1.125"
        >
          <RoomQuestionPanelAi
            questionWithCode={codeEditorEnabled}
            roomQuestionsLoading={roomQuestionsLoading}
            roomQuestions={roomQuestions?.sort(sortRoomQuestion) || []}
            initialQuestion={initialQuestion}
          >
            <div className="flex flex-col items-center justify-center">
              <video
                ref={userVideoMainContent}
                className="w-14.5 h-14.5 rounded-full videochat-video object-cover z-1"
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
              <div className="flex absolute" style={{ bottom: '-1.25rem' }}>
                <Button
                  variant="invertedAlternative"
                  className="min-w-unset w-2.5 h-2.5 p-0 z-1"
                  onClick={handleMicSwitch}
                >
                  <Icon size="s" name={micEnabled ? IconNames.MicOn : IconNames.MicOff} />
                </Button>
                <Gap sizeRem={0.5} horizontal />
                <Button
                  variant="invertedAlternative"
                  className="min-w-unset w-2.5 h-2.5 p-0 z-1"
                  onClick={handleCameraSwitch}
                >
                  <Icon size="s" name={cameraEnabled ? IconNames.VideocamOn : IconNames.VideocamOff} />
                </Button>
              </div>
            </div>
          </RoomQuestionPanelAi>
          <Gap sizeRem={3.375} />
          <div className="flex">
            <ContextMenu
              toggleContent={
                <Button
                  variant="invertedAlternative"
                  className="min-w-unset w-2.5 h-2.5 p-0"
                  onClick={currentUserExpert ? () => { } : handleLeaveRoom}
                >
                  <Icon size="s" name={IconNames.Call} />
                </Button>
              }
              translateRem={{ x: -14.25, y: -6.75 }}
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
            <Gap sizeRem={0.5} horizontal />
            <Button
              variant="invertedAlternative"
              className="min-w-unset w-2.5 h-2.5 p-0"
              onClick={handleSettingsOpen}
            >
              <Icon size="s" name={IconNames.Settings} />
            </Button>
            <Gap sizeRem={0.5} horizontal />
            <Button
              variant="invertedAlternative"
              className="min-w-unset w-2.5 h-2.5 p-0"
              onClick={handleInvitationsOpen}
            >
              <Icon size="s" name={IconNames.PersonAdd} />
            </Button>
          </div>
          <Gap sizeRem={1.875} />
        </div>
      </div>

      <div
        className="absolute videochat-field overflow-auto right-1"
        style={{
          height: 'calc(100% - 0.75rem)',
        }}
      >
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
