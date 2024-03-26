import { FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import { useParams, Navigate } from 'react-router-dom';
import useWebSocket from 'react-use-websocket';
import toast from 'react-hot-toast';
import {
  GetRoomParticipantParams,
  GetRoomQuestionsBody,
  roomQuestionApiDeclaration,
  roomsApiDeclaration,
} from '../../apiDeclarations';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { REACT_APP_WS_URL } from '../../config';
import { IconNames, pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { useCommunist } from '../../hooks/useCommunist';
import { RoomParticipant, RoomQuestion, RoomState, Room as RoomType } from '../../types/room';
import { ActionModal } from '../../components/ActionModal/ActionModal';
import { Reactions } from './components/Reactions/Reactions';
import { ActiveQuestion } from './components/ActiveQuestion/ActiveQuestion';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { VideoChat } from './components/VideoChat/VideoChat';
import { SwitchButton } from './components/VideoChat/SwitchButton';
import { Link } from 'react-router-dom';
import { ThemeSwitchMini } from '../../components/ThemeSwitchMini/ThemeSwitchMini';
import { EnterVideoChatModal } from './components/VideoChat/EnterVideoChatModal';
import { Devices, useUserStream } from './hooks/useUserStream';
import { useSpeechRecognition } from './hooks/useSpeechRecognition';
import { useUnreadChatMessages } from './hooks/useUnreadChatMessages';
import { Localization } from '../../localization';
import { MessagePage } from '../../components/MessagePage/MessagePage';
import { ThemedIcon } from './components/ThemedIcon/ThemedIcon';

import './Room.css';

const connectingReadyState = 0;

const enableDisableUserTrack = (stream: MediaStream, kind: string, enabled: boolean) => {
  const track = stream.getTracks().find(track => track.kind === kind);
  if (!track) {
    return;
  }
  track.enabled = enabled;
};

export const Room: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { getCommunist } = useCommunist();
  const communist = getCommunist();
  let { id } = useParams();
  const socketUrl = `${REACT_APP_WS_URL}/ws?Authorization=${communist}&roomId=${id}`;
  const { lastMessage, readyState, sendMessage } = useWebSocket(socketUrl);
  const wsClosed = readyState === 3 || readyState === 2;
  const [roomInReview, setRoomInReview] = useState(false);
  const [reactionsVisible, setReactionsVisible] = useState(false);
  const [currentQuestionId, setCurrentQuestionId] = useState<RoomQuestion['id']>();
  const [currentQuestion, setCurrentQuestion] = useState<RoomQuestion>();
  const [messagesChatEnabled, setMessagesChatEnabled] = useState(false);
  const [welcomeScreen, setWelcomeScreen] = useState(true);
  const [micEnabled, setMicEnabled] = useState(true);
  const [cameraEnabled, setCameraEnabled] = useState(true);
  const [selectedDevices, setSelectedDevices] = useState<Devices | null>(null);
  const [recognitionEnabled, setRecognitionEnabled] = useState(false);
  const [peersLength, setPeersLength] = useState(0);
  const { userStream } = useUserStream({
    selectedDevices,
  });

  const handleVoiceRecognition = (transcript: string) => {
    sendMessage(JSON.stringify({
      Type: 'voice-recognition',
      Value: transcript,
    }));
  };

  const { recognitionNotSupported } = useSpeechRecognition({
    recognitionEnabled,
    onVoiceRecognition: handleVoiceRecognition,
  });
  const { unreadChatMessages } = useUnreadChatMessages({
    lastMessage,
    messagesChatEnabled,
  });

  const { apiMethodState, fetchData } = useApiMethod<RoomType, RoomType['id']>(roomsApiDeclaration.getById);
  const { process: { loading, error }, data: room } = apiMethodState;

  const {
    apiMethodState: apiRoomStartReviewMethodState,
    fetchData: fetchRoomStartReview,
  } = useApiMethod<unknown, RoomType['id']>(roomsApiDeclaration.startReview);
  const {
    process: { loading: roomStartReviewLoading, error: roomStartReviewError },
  } = apiRoomStartReviewMethodState;

  const {
    apiMethodState: apiRoomQuestions,
    fetchData: getRoomQuestions,
  } = useApiMethod<Array<RoomQuestion>, GetRoomQuestionsBody>(roomQuestionApiDeclaration.getRoomQuestions);
  const {
    data: roomQuestions,
  } = apiRoomQuestions;

  const {
    apiMethodState: apiRoomParticipantState,
    fetchData: getRoomParticipant,
  } = useApiMethod<RoomParticipant, GetRoomParticipantParams>(roomsApiDeclaration.getParticipant);
  const {
    data: roomParticipant, process: { loading: roomParticipantLoading, error: roomParticipantError },
  } = apiRoomParticipantState;

  const roomParticipantWillLoaded = roomParticipant === null && !roomParticipantError;

  const {
    apiMethodState: apiRoomStateState,
    fetchData: fetchRoomState,
  } = useApiMethod<RoomState, RoomType['id']>(roomsApiDeclaration.getState);
  const {
    process: { loading: loadingRoomState, error: errorRoomState },
    data: roomState,
  } = apiRoomStateState;

  const currentUserExpert = roomParticipant?.userType === 'Expert';
  const currentUserExaminee = roomParticipant?.userType === 'Examinee';
  const viewerMode = !(currentUserExpert || currentUserExaminee);

  const updateQuestions = useCallback(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    getRoomQuestions({
      RoomId: id,
      States: ['Open', 'Closed', 'Active'],
    });
  }, [id, getRoomQuestions]);

  useEffect(() => {
    if (!auth?.id) {
      return;
    }
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchData(id);
    fetchRoomState(id);
    updateQuestions();
    getRoomParticipant({
      RoomId: id,
      UserId: auth.id,
    });
  }, [id, auth?.id, fetchData, fetchRoomState, updateQuestions, getRoomParticipant]);

  useEffect(() => {
    if (!room) {
      return;
    }
    if (room.roomStatus !== 'New') {
      setReactionsVisible(true);
    }
  }, [room]);

  useEffect(() => {
    if (!room || !roomQuestions) {
      return;
    }
    const activeQuestion = roomQuestions.find(question => question.state === 'Active');
    if (!activeQuestion) {
      return;
    }
    setCurrentQuestion(activeQuestion);
  }, [room, roomQuestions]);

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    if (!lastMessage || !auth?.nickname) {
      return;
    }
    try {
      const parsedData = JSON.parse(lastMessage?.data);
      switch (parsedData?.Type) {
        case 'ChatMessage':
          const message = parsedData?.Value?.Message;
          const nickname = parsedData?.Value?.Nickname;
          if (typeof message !== 'string') {
            return;
          }
          if (message.includes(auth.nickname)) {
            toast(`${nickname}: ${message}`, { icon: '💬' });
          }
          break;
        case 'ChangeRoomStatus':
          const newStatus: RoomType['roomStatus'] = 'New';
          const reviewStatus: RoomType['roomStatus'] = 'Review';
          if (parsedData?.Value === reviewStatus) {
            setRoomInReview(true);
          }
          if (parsedData?.Value !== newStatus) {
            setReactionsVisible(true);
          }
          break;
        case 'ChangeRoomQuestionState':
          if (parsedData.Value.NewState !== 'Active') {
            break;
          }
          setCurrentQuestionId(parsedData.Value.QuestionId);
          break;
        case 'AddRoomQuestion':
          updateQuestions();
          break;
        default:
          break;
      }
    } catch { }
  }, [id, auth, lastMessage, updateQuestions]);

  useEffect(() => {
    if (!currentQuestionId) {
      return;
    }
    updateQuestions();
  }, [id, currentQuestionId, updateQuestions]);

  const handleStartReviewRoom = useCallback(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomStartReview(id);
  }, [id, fetchRoomStartReview]);

  const handleMessagesChatSwitch = () => {
    setMessagesChatEnabled(!messagesChatEnabled);
  };

  const loaders = [
    {},
    {},
    { height: '890px' }
  ];

  const handleMediaSelect = useCallback((devices: Devices) => {
    setSelectedDevices(devices);
    setMicEnabled(true);
    setCameraEnabled(true);
  }, []);

  const handleWelcomeScreenClose = () => {
    setWelcomeScreen(false);
    sendMessage(JSON.stringify({
      Type: "join video chat",
    }));
    if (viewerMode) {
      return;
    }
    setRecognitionEnabled(true);
  };

  const handleCameraSwitch = useCallback(() => {
    if (userStream) {
      enableDisableUserTrack(userStream, 'video', !cameraEnabled);
    }
    setCameraEnabled(!cameraEnabled);
  }, [userStream, cameraEnabled]);

  const handleMicSwitch = useCallback(() => {
    if (userStream) {
      enableDisableUserTrack(userStream, 'audio', !micEnabled);
    }
    setMicEnabled(!micEnabled);
  }, [userStream, micEnabled]);

  useEffect(() => {
    if (welcomeScreen || viewerMode) {
      return;
    }
    setRecognitionEnabled(micEnabled);
  }, [welcomeScreen, viewerMode, micEnabled]);

  const handleVoiceRecognitionSwitch = useCallback(() => {
    setRecognitionEnabled(!recognitionEnabled);
  }, [recognitionEnabled]);

  if (roomInReview && id) {
    return <Navigate to={pathnames.roomAnalyticsSummary.replace(':id', id)} replace />;
  }

  if (wsClosed) {
    return (
      <MessagePage title={Localization.ConnectionError} message={Localization.RoomConnectionError}>
        <Link to={pathnames.rooms}>
          <button>{Localization.Exit}</button>
        </Link>
      </MessagePage>
    );
  }

  return (
    <MainContentWrapper withMargin className="room-wrapper">
      <EnterVideoChatModal
        open={welcomeScreen}
        loading={loading || roomParticipantLoading || roomParticipantWillLoaded || readyState === connectingReadyState}
        viewerMode={viewerMode}
        roomName={room?.name}
        userStream={userStream}
        micEnabled={micEnabled}
        cameraEnabled={cameraEnabled}
        onSelect={handleMediaSelect}
        onClose={handleWelcomeScreenClose}
        onMicSwitch={handleMicSwitch}
        onCameraSwitch={handleCameraSwitch}
      />
      <ProcessWrapper
        loading={loading}
        loadingPrefix={Localization.LoadingRoom}
        loaders={loaders}
        error={error}
        errorPrefix={Localization.ErrorLoadingRoom}
      >
        <div className="room-page">
          <div className="room-page-main">
            <div className="room-page-header">
              <div>
                <span
                  className={`room-page-header-caption ${viewerMode ? 'room-page-header-caption-viewer' : ''}`}
                >
                  <h3>{room?.name}</h3>
                  <span className='room-page-header-viewers'><ThemedIcon name={IconNames.People} /> {peersLength + 1}</span>
                  {viewerMode && (
                    <div
                      className="room-page-header-question-viewer"
                    >
                      {Localization.ActiveQuestion}: {currentQuestion?.value}
                    </div>
                  )}
                </span>
              </div>

              {!viewerMode && (
                <div className="reactions-field">
                  <ActiveQuestion
                    room={room}
                    roomQuestions={roomQuestions || []}
                    initialQuestionText={currentQuestion?.value}
                  />
                </div>
              )}
              {!reactionsVisible && (
                <div>{Localization.WaitingRoom}</div>
              )}
              <div className={`actions-field ${viewerMode ? 'actions-field-viewer' : ''}`}>
                {!viewerMode && (
                  <div className='start-room-review'>
                    <ActionModal
                      title={Localization.StartReviewRoomModalTitle}
                      openButtonCaption={Localization.StartReviewRoom}
                      loading={roomStartReviewLoading}
                      loadingCaption={Localization.CloseRoomLoading}
                      error={roomStartReviewError}
                      onAction={handleStartReviewRoom}
                    />
                  </div>
                )}
                <ThemeSwitchMini />
              </div>
            </div>
            <div className="room-page-main-content">
              {loadingRoomState && <div>{Localization.LoadingRoomState}...</div>}
              {errorRoomState && <div>{Localization.ErrorLoadingRoomState}...</div>}
              <VideoChat
                roomState={roomState}
                viewerMode={viewerMode}
                lastWsMessage={lastMessage}
                messagesChatEnabled={messagesChatEnabled}
                userStream={userStream}
                videoTrackEnabled={cameraEnabled}
                onSendWsMessage={sendMessage}
                onUpdatePeersLength={setPeersLength}
              />
            </div>
          </div>
          <div className="room-tools-container">
            {!viewerMode && (
              <div className="room-tools room-tools-left">
                <SwitchButton
                  enabled={micEnabled}
                  iconEnabledName={IconNames.MicOn}
                  iconDisabledName={IconNames.MicOff}
                  disabledColor
                  subCaption={Localization.Microphone}
                  onClick={handleMicSwitch}
                />
                <SwitchButton
                  enabled={cameraEnabled}
                  iconEnabledName={IconNames.VideocamOn}
                  iconDisabledName={IconNames.VideocamOff}
                  disabledColor
                  subCaption={Localization.Camera}
                  onClick={handleCameraSwitch}
                />
                {recognitionNotSupported ? (
                  <div>{Localization.VoiceRecognitionNotSupported}</div>
                ) : (
                  <SwitchButton
                    enabled={recognitionEnabled}
                    iconEnabledName={IconNames.RecognitionOn}
                    iconDisabledName={IconNames.RecognitionOff}
                    subCaption={Localization.VoiceRecognition}
                    disabledColor
                    onClick={handleVoiceRecognitionSwitch}
                  />
                )}
              </div>
            )}
            <div className="room-tools room-tools-center">
              <SwitchButton
                counter={unreadChatMessages}
                enabled={!messagesChatEnabled}
                iconEnabledName={IconNames.Chat}
                iconDisabledName={IconNames.Chat}
                subCaption={Localization.Chat}
                onClick={handleMessagesChatSwitch}
              />
              {reactionsVisible && (
                <Reactions
                  room={room}
                  roomState={roomState}
                  roles={auth?.roles || []}
                  participantType={roomParticipant?.userType || null}
                  lastWsMessage={lastMessage}
                />
              )}
            </div>
            <div className="room-tools room-tools-right">
              <Link to={pathnames.rooms}>
                <button>{Localization.Exit}</button>
              </Link>
            </div>
          </div>
        </div>
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
