import { FunctionComponent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { useParams, Navigate, useNavigate } from 'react-router-dom';
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
import { Reactions } from './components/Reactions/Reactions';
import { ActiveQuestion } from './components/ActiveQuestion/ActiveQuestion';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { VideoChat } from './components/VideoChat/VideoChat';
import { Link } from 'react-router-dom';
import { EnterVideoChatModal } from './components/VideoChat/EnterVideoChatModal';
import { useUserStreams } from './hooks/useUserStreams';
import { useSpeechRecognition } from './hooks/useSpeechRecognition';
import { useUnreadChatMessages } from './hooks/useUnreadChatMessages';
import { useScreenStream } from './hooks/useScreenStream';
import { LocalizationKey } from '../../localization';
import { MessagePage } from '../../components/MessagePage/MessagePage';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Invitations } from './components/Invitations/Invitations';
import { UserType } from '../../types/user';
import { useEventsState } from './hooks/useEventsState';
import { RoomTimer } from './components/RoomTimer/RoomTimer';
import { CodeEditorLang } from '../../types/question';
import { Button } from '../../components/Button/Button';
import { RoomToolsPanel } from './components/RoomToolsPanel/RoomToolsPanel';
import { SwitcherButton } from '../../components/SwitcherButton/SwitcherButton';
import { Gap } from '../../components/Gap/Gap';

import './Room.css';

const connectingReadyState = 0;

export const Room: FunctionComponent = () => {
  const navigate = useNavigate();
  const auth = useContext(AuthContext);
  const { getCommunist } = useCommunist();
  const communist = getCommunist();
  let { id } = useParams();
  const { [inviteParamName]: inviteParam } = useParams();

  const {
    apiMethodState: apiApplyRoomInviteState,
    fetchData: fetchApplyRoomInvite,
  } = useApiMethod<RoomInvite, ApplyRoomInviteBody>(roomInviteApiDeclaration.apply);
  const {
    process: { loading: applyRoomInviteLoading, error: applyRoomInviteError },
    data: applyRoomInviteData,
  } = apiApplyRoomInviteState;

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
  const [invitationsOpen, setInvitationsOpen] = useState(false);
  const socketUrl = `${REACT_APP_WS_URL}/ws?Authorization=${communist}&roomId=${id}`;
  const checkWebSocketReadyToConnect = () => {
    if (!inviteParam) {
      return true;
    }
    if (applyRoomInviteData) {
      return true;
    }
    return false;
  };
  const { lastMessage, readyState, sendMessage } = useWebSocket(checkWebSocketReadyToConnect() ? socketUrl : null);
  const wsClosed = readyState === 3 || readyState === 2;
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
  const codeEditorLanguage = String(eventsState[EventName.CodeEditorLanguage]) as CodeEditorLang;

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

  const handleSwitchMessagesChat = (index: number) => {
    setMessagesChatEnabled(index === 0);
  };

  const handleInvitationsOpen = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomInvites({
      roomId: id,
    });
    setInvitationsOpen(true);
  };

  const handleInvitationsClose = () => {
    setInvitationsOpen(false);
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

  const handleLeaveRoom = () => {
    navigate(pathnames.currentRooms);
  };

  const renderToolsPanel = () => {
    return (
      <RoomToolsPanel.Wrapper rightPos='18.5rem' bottomPos='1.5rem'>
        <RoomToolsPanel.SwitchButton
          enabled={micEnabled}
          iconEnabledName={IconNames.MicOn}
          iconDisabledName={IconNames.MicOff}
          onClick={handleMicSwitch}
          roundedTop
        />
        <Gap sizeRem={0.125} />
        <RoomToolsPanel.SwitchButton
          enabled={cameraEnabled}
          iconEnabledName={IconNames.VideocamOn}
          iconDisabledName={IconNames.VideocamOff}
          onClick={handleCameraSwitch}
          roundedBottom={recognitionNotSupported}
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
              roundedBottom
            />
          </>
        )}
        {reactionsVisible && (
          <>
            <Gap sizeRem={1.5} />
            <Reactions
              room={room}
              eventsState={eventsState}
              roles={auth?.roles || []}
              participantType={roomParticipant?.userType || null}
            />
          </>
        )}
        {!viewerMode && (
          <>
            <Gap sizeRem={0.125} />
            <RoomToolsPanel.SwitchButton
              enabled={true}
              iconEnabledName={IconNames.TV}
              iconDisabledName={IconNames.TV}
              onClick={handleScreenShare}
            />
            <Gap sizeRem={0.125} />
            <RoomToolsPanel.SwitchButton
              enabled={true}
              iconEnabledName={IconNames.PersonAdd}
              iconDisabledName={IconNames.PersonAdd}
              onClick={handleInvitationsOpen}
              roundedBottom
            />
          </>
        )}
        <Gap sizeRem={1.5} />
        <RoomToolsPanel.SwitchButton
          enabled={true}
          iconEnabledName={IconNames.Call}
          iconDisabledName={IconNames.Call}
          onClick={handleLeaveRoom}
          roundedTop
          roundedBottom
          danger
        />
      </RoomToolsPanel.Wrapper>
    );
  };

  if (roomInReview && id) {
    return <Navigate to={pathnames.roomAnalyticsSummary.replace(':id', id)} replace />;
  }

  if (wsClosed) {
    return (
      <MessagePage title={localizationCaptions[LocalizationKey.ConnectionError]} message={localizationCaptions[LocalizationKey.RoomConnectionError]}>
        <Link to={pathnames.currentRooms}>
          <Button>{localizationCaptions[LocalizationKey.Exit]}</Button>
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
      <Invitations
        open={invitationsOpen}
        roomId={id || ''}
        roomInvitesData={roomInvitesData}
        roomInvitesError={roomInvitesError || generateRoomInviteError || generateRoomAllInvitesError}
        roomInvitesLoading={roomInvitesLoading || generateRoomInviteLoading || generateRoomAllInvitesLoading}
        onRequestClose={handleInvitationsClose}
        onGenerateInvite={handleInviteGenerate}
        onGenerateAllInvites={handleInvitesAllGenerate}
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
            <div className="room-page-header justify-between">
              <div>
                <span
                  className={`room-page-header-caption ${viewerMode ? 'room-page-header-caption-viewer' : ''}`}
                >
                  <div className='room-page-header-wrapper flex items-center'>
                    <div className='w-2.375 pr-1'>
                      <img className='w-2.375 h-2.375 rounded-0.375' src='/logo192.png' alt='site logo' />
                    </div>
                    <h3>{room?.name}</h3>
                  </div>
                </span>
              </div>
              <div className='flex'>
                {!!roomTimer?.startTime && (
                  <>
                    <RoomTimer durationSec={roomTimer.durationSec} startTime={roomTimer.startTime} />
                    <Gap sizeRem={0.5} horizontal />
                  </>
                )}
                {!reactionsVisible && (
                  <div>{localizationCaptions[LocalizationKey.WaitingRoom]}</div>
                )}
                <SwitcherButton
                  captions={[
                    `${localizationCaptions[LocalizationKey.Chat]} ${unreadChatMessages || ''}`,
                    `${localizationCaptions[LocalizationKey.RoomParticipants]} ${peersLength + 1}`,
                  ]}
                  activeIndex={messagesChatEnabled ? 0 : 1}
                  variant='alternative'
                  onClick={handleSwitchMessagesChat}
                />
              </div>
            </div>
            <div className="room-page-main-content">
              <div className='room-columns'>
                {loadingRoomState && <div>{localizationCaptions[LocalizationKey.LoadingRoomState]}...</div>}
                {errorRoomState && <div>{localizationCaptions[LocalizationKey.ErrorLoadingRoomState]}...</div>}
                <div className='videochat-field !w-21 bg-wrap rounded-1.125'>
                  <div className='py-1.5 px-1.25'>
                    <ActiveQuestion
                      room={room}
                      roomQuestions={roomQuestions || []}
                      initialQuestion={currentQuestion}
                      readOnly={viewerMode}
                    />
                  </div>
                </div>
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
                  renderToolsPanel={renderToolsPanel}
                />
              </div>
            </div>
          </div>
        </div>
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
