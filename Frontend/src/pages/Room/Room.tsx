import { FunctionComponent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { useParams, Navigate } from 'react-router-dom';
import useWebSocket from 'react-use-websocket';
import toast from 'react-hot-toast';
import {
  ApplyRoomInviteBody,
  GenerateRoomInviteBody,
  GetRoomParticipantParams,
  GetRoomQuestionsBody,
  RoomIdParam,
  roomInviteApiDeclaration,
  roomQuestionApiDeclaration,
  roomsApiDeclaration,
} from '../../apiDeclarations';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { REACT_APP_WS_URL } from '../../config';
import { EventName, IconNames, inviteParamName, pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { useCommunist } from '../../hooks/useCommunist';
import { RoomInvite, RoomParticipant, RoomQuestion, RoomState, Room as RoomType } from '../../types/room';
import { ActionModal } from '../../components/ActionModal/ActionModal';
import { Reactions } from './components/Reactions/Reactions';
import { ActiveQuestion } from './components/ActiveQuestion/ActiveQuestion';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { VideoChat } from './components/VideoChat/VideoChat';
import { SwitchButton } from './components/VideoChat/SwitchButton';
import { Link } from 'react-router-dom';
import { EnterVideoChatModal } from './components/VideoChat/EnterVideoChatModal';
import { useUserStreams } from './hooks/useUserStreams';
import { useSpeechRecognition } from './hooks/useSpeechRecognition';
import { useUnreadChatMessages } from './hooks/useUnreadChatMessages';
import { useScreenStream } from './hooks/useScreenStream';
import { LocalizationKey } from '../../localization';
import { MessagePage } from '../../components/MessagePage/MessagePage';
import { ThemedIcon } from './components/ThemedIcon/ThemedIcon';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Invitations } from './components/Invitations/Invitations';
import { UserType } from '../../types/user';
import { useEventsState } from './hooks/useEventsState';
import { RoomTimer } from './components/RoomTimer/RoomTimer';

import './Room.css';

const connectingReadyState = 0;

export const Room: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { getCommunist } = useCommunist();
  const communist = getCommunist();
  let { id } = useParams();
  const { [inviteParamName]: inviteParam } = useParams();
  const socketUrl = `${REACT_APP_WS_URL}/ws?Authorization=${communist}&roomId=${id}`;
  const { lastMessage, readyState, sendMessage } = useWebSocket(socketUrl);
  const wsClosed = readyState === 3 || readyState === 2;
  const [roomInReview, setRoomInReview] = useState(false);
  const [reactionsVisible, setReactionsVisible] = useState(false);
  const [currentQuestionId, setCurrentQuestionId] = useState<RoomQuestion['id']>();
  const [currentQuestion, setCurrentQuestion] = useState<RoomQuestion>();
  const [wsRoomTimer, setWsRoomTimer] = useState<RoomType['timer'] | null>(null);
  const [messagesChatEnabled, setMessagesChatEnabled] = useState(false);
  const [welcomeScreen, setWelcomeScreen] = useState(true);
  const micDisabledAutomatically = useRef(false);
  const [recognitionEnabled, setRecognitionEnabled] = useState(false);
  const [peersLength, setPeersLength] = useState(0);
  const {
    devices,
    userAudioStream,
    userVideoStream,
    updateDevices,
    setSelectedCameraId,
    setSelectedMicId,
    cameraEnabled,
    micEnabled,
    setCameraEnabled,
    setMicEnabled,
  } = useUserStreams();
  const { screenStream, requestScreenStream } = useScreenStream();
  const localizationCaptions = useLocalizationCaptions();

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

  const {
    apiMethodState: apiRoomInvitesState,
    fetchData: fetchRoomInvites,
  } = useApiMethod<RoomInvite[], RoomIdParam>(roomInviteApiDeclaration.get);
  const {
    process: { loading: roomInvitesLoading, error: roomInvitesError },
    data: roomInvitesData,
  } = apiRoomInvitesState;

  const {
    apiMethodState: apiApplyRoomInviteState,
    fetchData: fetchApplyRoomInvite,
  } = useApiMethod<RoomInvite, ApplyRoomInviteBody>(roomInviteApiDeclaration.apply);
  const {
    process: { loading: applyRoomInviteLoading, error: applyRoomInviteError },
    data: applyRoomInviteData,
  } = apiApplyRoomInviteState;

  const {
    apiMethodState: apiGenerateRoomInviteState,
    fetchData: fetchGenerateRoomInvite,
  } = useApiMethod<RoomInvite, GenerateRoomInviteBody>(roomInviteApiDeclaration.generate);
  const {
    process: { loading: generateRoomInviteLoading, error: generateRoomInviteError },
    data: generateRoomInviteData,
  } = apiGenerateRoomInviteState;

  const {
    apiMethodState: apiGenerateRoomAllInvitesState,
    fetchData: fetchGenerateRoomAllInvites,
  } = useApiMethod<RoomInvite, RoomIdParam>(roomInviteApiDeclaration.generateAll);
  const {
    process: { loading: generateRoomAllInvitesLoading, error: generateRoomAllInvitesError },
    data: generateRoomAllInvitesData,
  } = apiGenerateRoomAllInvitesState;

  const roomParticipantWillLoaded = roomParticipant === null && !roomParticipantError;

  const {
    apiMethodState: apiRoomStateState,
    fetchData: fetchRoomState,
  } = useApiMethod<RoomState, RoomType['id']>(roomsApiDeclaration.getState);
  const {
    process: { loading: loadingRoomState, error: errorRoomState },
    data: roomState,
  } = apiRoomStateState;

  const roomTimer = wsRoomTimer || room?.timer;
  const eventsState = useEventsState({ roomState, lastWsMessage: lastMessage });
  const codeEditorEnabled = !!eventsState[EventName.CodeEditor];
  const codeEditorLanguage = String(eventsState[EventName.CodeEditorLanguage]);

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
    if (!inviteParam) {
      return;
    }
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchApplyRoomInvite({
      inviteId: inviteParam,
      roomId: id,
    });
  }, [inviteParam, id, fetchApplyRoomInvite]);

  useEffect(() => {
    if (!generateRoomInviteData && !generateRoomAllInvitesData) {
      return;
    }
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomInvites({
      roomId: id,
    });
  }, [generateRoomInviteData, generateRoomAllInvitesData, id, fetchRoomInvites]);

  useEffect(() => {
    if (inviteParam && !applyRoomInviteData) {
      return;
    }
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
  }, [id, auth?.id, inviteParam, applyRoomInviteData, fetchData, fetchRoomState, updateQuestions, getRoomParticipant]);

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
        case 'StartRoomTimer':
          setWsRoomTimer({
            durationSec: parsedData.Value.DurationSec,
            startTime: parsedData.Value.StartTime,
          });
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

  const handleScreenShare = () => {
    requestScreenStream();
  };

  const loaders = [
    {},
    {},
    { height: '890px' }
  ];

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
    setCameraEnabled(!cameraEnabled);
  }, [cameraEnabled, setCameraEnabled]);

  const enableDisableMic = useCallback((enabled: boolean) => {
    setMicEnabled(enabled);
  }, [setMicEnabled]);

  const handleMicSwitch = useCallback(() => {
    if (micEnabled) {
      micDisabledAutomatically.current = false;
    }
    enableDisableMic(!micEnabled);
  }, [micEnabled, enableDisableMic]);

  useEffect(() => {
    if (welcomeScreen || viewerMode) {
      return;
    }
    setRecognitionEnabled(micEnabled);
  }, [welcomeScreen, viewerMode, micEnabled]);

  const muteMic = useCallback(() => {
    enableDisableMic(false);
  }, [enableDisableMic]);

  const unmuteMic = useCallback(() => {
    enableDisableMic(true);
  }, [enableDisableMic]);

  const handleVoiceRecognitionSwitch = useCallback(() => {
    setRecognitionEnabled(!recognitionEnabled);
  }, [recognitionEnabled]);

  const handleInvitesOpen = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomInvites({
      roomId: id,
    });
  };

  const handleInviteGenerate = (participantType: UserType) => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchGenerateRoomInvite({
      roomId: id,
      participantType,
    });
  };

  const handleInvitesAllGenerate = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchGenerateRoomAllInvites({
      roomId: id,
    });
  };

  if (roomInReview && id) {
    return <Navigate to={pathnames.roomAnalyticsSummary.replace(':id', id)} replace />;
  }

  if (wsClosed) {
    return (
      <MessagePage title={localizationCaptions[LocalizationKey.ConnectionError]} message={localizationCaptions[LocalizationKey.RoomConnectionError]}>
        <Link to={pathnames.rooms}>
          <button>{localizationCaptions[LocalizationKey.Exit]}</button>
        </Link>
      </MessagePage>
    );
  }

  return (
    <MainContentWrapper withMargin className="room-wrapper">
      <EnterVideoChatModal
        open={welcomeScreen}
        loading={loading || roomParticipantLoading || roomParticipantWillLoaded || applyRoomInviteLoading || readyState === connectingReadyState}
        viewerMode={viewerMode}
        roomName={room?.name}
        devices={devices}
        setSelectedCameraId={setSelectedCameraId}
        setSelectedMicId={setSelectedMicId}
        updateDevices={updateDevices}
        error={applyRoomInviteError && localizationCaptions[LocalizationKey.ErrorApplyRoomInvite]}
        userVideoStream={userVideoStream}
        userAudioStream={userAudioStream}
        micEnabled={micEnabled}
        cameraEnabled={cameraEnabled}
        onClose={handleWelcomeScreenClose}
        onMicSwitch={handleMicSwitch}
        onCameraSwitch={handleCameraSwitch}
      />
      <ProcessWrapper
        loading={loading}
        loadingPrefix={localizationCaptions[LocalizationKey.LoadingRoom]}
        loaders={loaders}
        error={error}
        errorPrefix={localizationCaptions[LocalizationKey.ErrorLoadingRoom]}
      >
        <div className="room-page">
          <div className="room-page-main">
            <div className="room-page-header">
              <div>
                <span
                  className={`room-page-header-caption ${viewerMode ? 'room-page-header-caption-viewer' : ''}`}
                >
                  <div className='room-page-header-wrapper'>
                    <h3>{room?.name}</h3>
                    <span className='room-page-header-viewers'><ThemedIcon name={IconNames.People} /> {peersLength + 1}</span>
                  </div>
                  {viewerMode && (
                    <div
                      className="room-page-header-question-viewer"
                    >
                      {localizationCaptions[LocalizationKey.ActiveQuestion]}: {currentQuestion?.value}
                    </div>
                  )}
                </span>
              </div>
              {(viewerMode && !!(roomTimer?.startTime)) && (
                <RoomTimer durationSec={roomTimer.durationSec} startTime={roomTimer.startTime} />
              )}

              {!viewerMode && (
                <div className="reactions-field">
                  <ActiveQuestion
                    room={room}
                    roomQuestions={roomQuestions || []}
                    initialQuestionText={currentQuestion?.value}
                  />
                  {!!(roomTimer?.startTime) && (
                    <RoomTimer durationSec={roomTimer.durationSec} startTime={roomTimer.startTime} />
                  )}
                </div>
              )}
              {!reactionsVisible && (
                <div>{localizationCaptions[LocalizationKey.WaitingRoom]}</div>
              )}
              <div className={`actions-field ${viewerMode ? 'actions-field-viewer' : ''}`}>
                {!viewerMode && (
                  <Invitations
                    roomId={id || ''}
                    roomInvitesData={roomInvitesData}
                    roomInvitesError={roomInvitesError || generateRoomInviteError || generateRoomAllInvitesError}
                    roomInvitesLoading={roomInvitesLoading || generateRoomInviteLoading || generateRoomAllInvitesLoading}
                    onOpen={handleInvitesOpen}
                    onGenerateInvite={handleInviteGenerate}
                    onGenerateAllInvites={handleInvitesAllGenerate}
                  />
                )}
                {!viewerMode && (
                  <div className='start-room-review'>
                    <ActionModal
                      title={localizationCaptions[LocalizationKey.StartReviewRoomModalTitle]}
                      openButtonCaption={localizationCaptions[LocalizationKey.StartReviewRoom]}
                      loading={roomStartReviewLoading}
                      loadingCaption={localizationCaptions[LocalizationKey.CloseRoomLoading]}
                      error={roomStartReviewError}
                      onAction={handleStartReviewRoom}
                    />
                  </div>
                )}
              </div>
            </div>
            <div className="room-page-main-content">
              {loadingRoomState && <div>{localizationCaptions[LocalizationKey.LoadingRoomState]}...</div>}
              {errorRoomState && <div>{localizationCaptions[LocalizationKey.ErrorLoadingRoomState]}...</div>}
              <VideoChat
                roomState={roomState}
                viewerMode={viewerMode}
                lastWsMessage={lastMessage}
                messagesChatEnabled={messagesChatEnabled}
                codeEditorEnabled={codeEditorEnabled}
                codeEditorLanguage={codeEditorLanguage}
                userVideoStream={userVideoStream}
                userAudioStream={userAudioStream}
                screenStream={screenStream}
                micDisabledAutomatically={micDisabledAutomatically}
                onSendWsMessage={sendMessage}
                onUpdatePeersLength={setPeersLength}
                onMuteMic={muteMic}
                onUnmuteMic={unmuteMic}
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
                  subCaption={localizationCaptions[LocalizationKey.Microphone]}
                  onClick={handleMicSwitch}
                />
                <SwitchButton
                  enabled={cameraEnabled}
                  iconEnabledName={IconNames.VideocamOn}
                  iconDisabledName={IconNames.VideocamOff}
                  disabledColor
                  subCaption={localizationCaptions[LocalizationKey.Camera]}
                  onClick={handleCameraSwitch}
                />
                {recognitionNotSupported ? (
                  <div>{localizationCaptions[LocalizationKey.VoiceRecognitionNotSupported]}</div>
                ) : (
                  <SwitchButton
                    enabled={recognitionEnabled}
                    htmlDisabled={!micEnabled}
                    iconEnabledName={IconNames.RecognitionOn}
                    iconDisabledName={IconNames.RecognitionOff}
                    subCaption={localizationCaptions[LocalizationKey.VoiceRecognition]}
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
                subCaption={localizationCaptions[LocalizationKey.Chat]}
                onClick={handleMessagesChatSwitch}
              />
              {reactionsVisible && (
                <Reactions
                  room={room}
                  eventsState={eventsState}
                  roles={auth?.roles || []}
                  participantType={roomParticipant?.userType || null}
                  lastWsMessage={lastMessage}
                />
              )}
              {!viewerMode && (
                <SwitchButton
                  enabled={true}
                  iconEnabledName={IconNames.TV}
                  iconDisabledName={IconNames.TV}
                  subCaption={localizationCaptions[LocalizationKey.ScreenShare]}
                  onClick={handleScreenShare}
                />
              )}
            </div>
            <div className="room-tools room-tools-right">
              <Link to={pathnames.rooms}>
                <button>{localizationCaptions[LocalizationKey.Exit]}</button>
              </Link>
            </div>
          </div>
        </div>
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
