import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import {
  useParams,
  Navigate,
  useNavigate,
  generatePath,
} from 'react-router-dom';
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
import { VITE_WS_URL } from '../../config';
import {
  EventName,
  IconNames,
  inviteParamName,
  pathnames,
} from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import {
  RoomInvite,
  RoomParticipant,
  RoomQuestion,
  RoomState,
  Room as RoomType,
} from '../../types/room';
import { RoomQuestionPanel } from './components/RoomQuestionPanel/RoomQuestionPanel';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { VideoChat } from './components/VideoChat/VideoChat';
import { VideoChatAi } from './components/VideoChat/VideoChatAi';
import { Link } from 'react-router-dom';
import { EnterVideoChatModal } from './components/VideoChat/EnterVideoChatModal';
import { useSpeechRecognition } from './hooks/useSpeechRecognition';
import { useUnreadChatMessages } from './hooks/useUnreadChatMessages';
// ScreenShare
// import { useScreenStream } from './hooks/useScreenStream';
import { LocalizationKey } from '../../localization';
import { MessagePage } from '../../components/MessagePage/MessagePage';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Invitations } from './components/Invitations/Invitations';
import { UserType } from '../../types/user';
import { useEventsState } from './hooks/useEventsState';
import { RoomTimer } from './components/RoomTimer/RoomTimer';
import { CodeEditorLang } from '../../types/question';
import { Button } from '../../components/Button/Button';
import { SwitcherButton } from '../../components/SwitcherButton/SwitcherButton';
import { Gap } from '../../components/Gap/Gap';
import { parseWsMessage } from './utils/parseWsMessage';
import { sortRoomQuestion } from '../../utils/sortRoomQestions';
import { UnreadChatMessagesCounter } from './components/UnreadChatMessagesCounter/UnreadChatMessagesCounter';
import { RoomSettings } from './components/RoomSettings/RoomSettings';
import { UserStreamsContext } from './context/UserStreamsContext';
import { RoomContext } from './context/RoomContext';
import { useVideoChat } from './components/VideoChat/hoks/useVideoChat';
import { useUserStreams } from './hooks/useUserStreams';
import { useRoomSounds } from './hooks/useRoomSounds';
import { AiAssistantScriptName } from './components/AiAssistant/AiAssistant';
import { mapInvitesForAiRoom } from '../../utils/mapInvitesForAiRoom';
import { Typography } from '../../components/Typography/Typography';
import { Icon } from './components/Icon/Icon';
import { getDevAuthorization } from '../../utils/devAuthorization';
import { DeviceContext } from '../../context/DeviceContext';

import './Room.css';

const connectingReadyState = 0;

const getCloseRedirectLink = (roomId: string, currentUserExpert: boolean) => {
  if (currentUserExpert) {
    return generatePath(pathnames.roomReview, { id: roomId });
  }
  return generatePath(pathnames.home, { redirect: '' });
};

export const Room: FunctionComponent = () => {
  const navigate = useNavigate();
  const auth = useContext(AuthContext);
  const device = useContext(DeviceContext);
  const { id } = useParams();
  const { [inviteParamName]: inviteParam } = useParams();

  const {
    apiMethodState: apiApplyRoomInviteState,
    fetchData: fetchApplyRoomInvite,
  } = useApiMethod<RoomInvite, ApplyRoomInviteBody>(
    roomInviteApiDeclaration.apply,
  );
  const {
    process: { loading: applyRoomInviteLoading, error: applyRoomInviteError },
    data: applyRoomInviteData,
  } = apiApplyRoomInviteState;

  const [roomInReview, setRoomInReview] = useState(false);
  const [roomClose, setRoomClose] = useState(false);
  const [reactionsVisible, setReactionsVisible] = useState(false);
  const [currentQuestionId, setCurrentQuestionId] =
    useState<RoomQuestion['id']>();
  const [currentQuestion, setCurrentQuestion] = useState<RoomQuestion>();
  const [wsRoomTimer, setWsRoomTimer] = useState<RoomType['timer'] | null>(
    null,
  );
  const [messagesChatEnabled, setMessagesChatEnabled] = useState(false);
  const [welcomeScreen, setWelcomeScreen] = useState(true);
  const [recognitionEnabled, setRecognitionEnabled] = useState(false);
  const [invitationsOpen, setInvitationsOpen] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [lastVoiceRecognition, setLastVoiceRecognition] = useState('');
  const [aiAssistantCurrentScript, setAiAssistantCurrentScript] =
    useState<AiAssistantScriptName>(AiAssistantScriptName.Idle);
  const devAuthorization =
    import.meta.env.MODE === 'development'
      ? `&Authorization=${getDevAuthorization()?.Authorization}`
      : '';
  const socketUrl = `${VITE_WS_URL}/ws?roomId=${id}${devAuthorization}`;
  const checkWebSocketReadyToConnect = () => {
    if (!inviteParam) {
      return true;
    }
    if (applyRoomInviteData) {
      return true;
    }
    return false;
  };
  const { lastMessage, readyState, sendMessage } = useWebSocket(
    checkWebSocketReadyToConnect() ? socketUrl : null,
  );
  const wsClosed = readyState === 3 || readyState === 2;
  const lastWsMessageParsed = useMemo(
    () => parseWsMessage(lastMessage),
    [lastMessage],
  );
  // ScreenShare
  // const { screenStream, requestScreenStream } = useScreenStream();
  const localizationCaptions = useLocalizationCaptions();

  const handleVoiceRecognition = (transcript: string) => {
    setLastVoiceRecognition(transcript);
    sendMessage(
      JSON.stringify({
        Type: 'voice-recognition',
        Value: transcript,
      }),
    );
  };

  const { recognitionNotSupported, recognitionNotAllowed } =
    useSpeechRecognition({
      recognitionEnabled,
      onVoiceRecognition: handleVoiceRecognition,
    });
  const { unreadChatMessages } = useUnreadChatMessages({
    lastWsMessageParsed,
    messagesChatEnabled,
    maxCount: 9,
  });

  const { apiMethodState, fetchData } = useApiMethod<RoomType, RoomType['id']>(
    roomsApiDeclaration.getById,
  );
  const {
    process: { loading, error },
    data: room,
  } = apiMethodState;

  const { apiMethodState: apiRoomQuestions, fetchData: getRoomQuestions } =
    useApiMethod<Array<RoomQuestion>, GetRoomQuestionsBody>(
      roomQuestionApiDeclaration.getRoomQuestions,
    );
  const {
    data: roomQuestions,
    process: { loading: roomQuestionsLoading },
  } = apiRoomQuestions;

  const {
    apiMethodState: apiRoomParticipantState,
    fetchData: getRoomParticipant,
  } = useApiMethod<RoomParticipant, GetRoomParticipantParams>(
    roomsApiDeclaration.getParticipant,
  );
  const {
    data: roomParticipant,
    process: { loading: roomParticipantLoading, error: roomParticipantError },
  } = apiRoomParticipantState;

  const { apiMethodState: apiRoomInvitesState, fetchData: fetchRoomInvites } =
    useApiMethod<RoomInvite[], RoomIdParam>(roomInviteApiDeclaration.get);
  const {
    process: { loading: roomInvitesLoading, error: roomInvitesError },
    data: roomInvitesData,
  } = apiRoomInvitesState;

  const {
    apiMethodState: apiGenerateRoomInviteState,
    fetchData: fetchGenerateRoomInvite,
  } = useApiMethod<RoomInvite, GenerateRoomInviteBody>(
    roomInviteApiDeclaration.generate,
  );
  const {
    process: {
      loading: generateRoomInviteLoading,
      error: generateRoomInviteError,
    },
    data: generateRoomInviteData,
  } = apiGenerateRoomInviteState;

  const {
    apiMethodState: apiGenerateRoomAllInvitesState,
    fetchData: fetchGenerateRoomAllInvites,
  } = useApiMethod<RoomInvite, RoomIdParam>(
    roomInviteApiDeclaration.generateAll,
  );
  const {
    process: {
      loading: generateRoomAllInvitesLoading,
      error: generateRoomAllInvitesError,
    },
    data: generateRoomAllInvitesData,
  } = apiGenerateRoomAllInvitesState;

  const roomParticipantWillLoaded =
    roomParticipant === null && !roomParticipantError;

  const { apiMethodState: apiRoomStateState, fetchData: fetchRoomState } =
    useApiMethod<RoomState, RoomType['id']>(roomsApiDeclaration.getState);
  const {
    process: { loading: loadingRoomState, error: errorRoomState },
    data: roomState,
  } = apiRoomStateState;

  const {
    apiMethodState: apiRoomStartReviewMethodState,
    fetchData: fetchRoomStartReview,
  } = useApiMethod<unknown, RoomType['id']>(roomsApiDeclaration.startReview);
  const {
    process: { loading: loadingRoomStartReview, error: errorRoomStartReview },
  } = apiRoomStartReviewMethodState;

  const roomTimer = wsRoomTimer || room?.timer;
  const eventsState = useEventsState({ roomState, lastWsMessage: lastMessage });
  const [codeEditorEnabled, setCodeEditorEnabled] = useState(false);
  const codeEditorLanguage = String(
    eventsState[EventName.CodeEditorLanguage],
  ) as CodeEditorLang;

  const currentUserExpert = roomParticipant?.userType === 'Expert';
  const currentUserExaminee = roomParticipant?.userType === 'Examinee';
  const viewerMode = !(currentUserExpert || currentUserExaminee);
  const aiRoom = !!room?.questionTree;
  const exitLink = aiRoom ? pathnames.roadmaps : pathnames.highlightRooms;

  const userStreams = useUserStreams();
  const { playJoinRoomSound, playChatMessageSound } = useRoomSounds();
  const { peers, videoOrder, peerToStream, allUsers, pinUser } = useVideoChat({
    viewerMode,
    lastWsMessageParsed,
    userAudioStream: userStreams.userAudioStream,
    userVideoStream: userStreams.userVideoStream,
    sendWsMessage: sendMessage,
    playJoinRoomSound,
  });

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
  }, [
    generateRoomInviteData,
    generateRoomAllInvitesData,
    id,
    fetchRoomInvites,
  ]);

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
  }, [
    id,
    auth?.id,
    inviteParam,
    applyRoomInviteData,
    fetchData,
    fetchRoomState,
    updateQuestions,
    getRoomParticipant,
  ]);

  useEffect(() => {
    if (!room) {
      return;
    }
    if (room.status !== 'New') {
      setReactionsVisible(true);
    }
  }, [room]);

  useEffect(() => {
    if (!roomState) {
      return;
    }
    setCodeEditorEnabled(roomState.codeEditor.enabled);
  }, [roomState]);

  useEffect(() => {
    if (!lastWsMessageParsed) {
      return;
    }
    try {
      switch (lastWsMessageParsed.Type) {
        case 'room-code-editor-enabled':
          setCodeEditorEnabled(lastWsMessageParsed.Value.Enabled);
          break;
        default:
          break;
      }
    } catch (err) {
      console.error(err);
    }
  }, [lastWsMessageParsed]);

  useEffect(() => {
    if (!room || !roomQuestions) {
      return;
    }
    const activeQuestion = roomQuestions.find(
      (question) => question.state === 'Active',
    );
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
        case 'ChatMessage': {
          const message = parsedData?.Value?.Message;
          const nickname = parsedData?.Value?.Nickname;
          if (typeof message !== 'string') {
            return;
          }
          if (message.includes(auth.nickname)) {
            toast(`${nickname}: ${message}`, { icon: '💬' });
          }
          playChatMessageSound();
          break;
        }
        case 'ChangeRoomStatus': {
          const newStatus: RoomType['status'] = 'New';
          const reviewStatus: RoomType['status'] = 'Review';
          const closeStatus: RoomType['status'] = 'Close';
          if (parsedData?.Value === closeStatus) {
            setRoomClose(true);
          }
          if (!aiRoom && parsedData?.Value === reviewStatus) {
            setRoomInReview(true);
          }
          if (parsedData?.Value !== newStatus) {
            setReactionsVisible(true);
          }
          break;
        }
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
    } catch (err) {
      console.error(err);
    }
  }, [id, auth, lastMessage, aiRoom, playChatMessageSound, updateQuestions]);

  useEffect(() => {
    if (!currentQuestionId) {
      return;
    }
    updateQuestions();
  }, [id, currentQuestionId, updateQuestions]);

  // ScreenShare
  // const handleScreenShare = () => {
  //   requestScreenStream();
  // };

  const loaders = [{}, {}, { height: '890px' }];

  const handleWelcomeScreenClose = () => {
    setWelcomeScreen(false);
    sendMessage(
      JSON.stringify({
        Type: 'join video chat',
      }),
    );
  };

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

  const handleSettingsOpen = () => {
    setSettingsOpen(true);
  };

  const handleInvitationsClose = () => {
    setInvitationsOpen(false);
  };

  const handleSettingsClose = () => {
    setSettingsOpen(false);
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
    navigate(exitLink);
  };

  const handleStartReviewRoom = () => {
    if (!room?.id) {
      console.warn('Room id not found');
      return;
    }
    fetchRoomStartReview(room.id);
  };

  if (roomClose && id) {
    return (
      <Navigate to={generatePath(pathnames.roomAnalytics, { id })} replace />
    );
  }

  if (roomInReview && id) {
    return (
      <Navigate to={getCloseRedirectLink(id, currentUserExpert)} replace />
    );
  }

  if (wsClosed) {
    return (
      <MessagePage
        title={localizationCaptions[LocalizationKey.ConnectionError]}
        message={localizationCaptions[LocalizationKey.RoomConnectionError]}
      >
        <Link to={exitLink}>
          <Button>{localizationCaptions[LocalizationKey.Exit]}</Button>
        </Link>
      </MessagePage>
    );
  }

  return (
    <RoomContext.Provider
      value={{
        room,
        roomParticipant,
        roomState,
        viewerMode,
        lastVoiceRecognition,
        lastWsMessageParsed,
        codeEditorEnabled,
        codeEditorLanguage,
        peers,
        pinUser: pinUser(),
        videoOrder,
        peerToStream,
        allUsers,
        aiAssistantScript: aiAssistantCurrentScript,
        recognitionEnabled,
        recognitionNotSupported,
        recognitionNotAllowed,
        sendWsMessage: sendMessage,
        setCodeEditorEnabled,
        setAiAssistantCurrentScript,
        setRecognitionEnabled,
      }}
    >
      <UserStreamsContext.Provider value={userStreams}>
        <MainContentWrapper withMargin={!aiRoom} className="room-wrapper">
          <EnterVideoChatModal
            aiRoom={aiRoom}
            open={welcomeScreen}
            loading={
              loading ||
              loadingRoomState ||
              roomParticipantLoading ||
              roomParticipantWillLoaded ||
              applyRoomInviteLoading ||
              readyState === connectingReadyState
            }
            error={
              applyRoomInviteError &&
              localizationCaptions[LocalizationKey.ErrorApplyRoomInvite]
            }
            onClose={handleWelcomeScreenClose}
          />
          {!welcomeScreen && (
            <>
              <Invitations
                open={invitationsOpen}
                roomId={id || ''}
                roomInvitesData={
                  !aiRoom
                    ? roomInvitesData
                    : mapInvitesForAiRoom(roomInvitesData || [])
                }
                roomInvitesError={
                  roomInvitesError ||
                  generateRoomInviteError ||
                  generateRoomAllInvitesError
                }
                roomInvitesLoading={
                  roomInvitesLoading ||
                  generateRoomInviteLoading ||
                  generateRoomAllInvitesLoading
                }
                onRequestClose={handleInvitationsClose}
                onGenerateInvite={handleInviteGenerate}
                onGenerateAllInvites={handleInvitesAllGenerate}
              />
              <RoomSettings
                open={settingsOpen}
                onRequestClose={handleSettingsClose}
              />
              <ProcessWrapper
                loading={loading}
                loadingPrefix={
                  localizationCaptions[LocalizationKey.LoadingRoom]
                }
                loaders={loaders}
                error={error}
                errorPrefix={
                  localizationCaptions[LocalizationKey.ErrorLoadingRoom]
                }
              >
                <div className="room-page">
                  <div className="room-page-main">
                    <div
                      className={`room-page-header justify-between ${aiRoom ? 'mt-[1.375rem] mb-[1.625rem] !pt-[0rem] !pb-[0rem]' : ''}`}
                    >
                      <div>
                        <span
                          className={`room-page-header-caption ${viewerMode ? 'room-page-header-caption-viewer' : ''}`}
                        >
                          <Link to={exitLink} className="no-underline">
                            <div className="room-page-header-wrapper flex items-center">
                              <div
                                className={`pr-[1rem] ${aiRoom ? 'w-[4.375rem] px-[1rem]' : 'w-[3.375rem]'}`}
                              >
                                <img
                                  className="w-[2.375rem] h-[2.375rem] rounded-[0.375rem]"
                                  src="/logo192.png"
                                  alt="site logo"
                                />
                              </div>
                              <h3>
                                <Typography size="xl" semibold>
                                  {room?.name}
                                </Typography>
                              </h3>
                            </div>
                          </Link>
                        </span>
                      </div>
                      {!aiRoom && (
                        <div className="flex">
                          {!!roomTimer?.startTime && (
                            <>
                              <RoomTimer
                                durationSec={roomTimer.durationSec}
                                startTime={roomTimer.startTime}
                              />
                              <Gap sizeRem={0.5} horizontal />
                            </>
                          )}
                          <SwitcherButton
                            items={[
                              {
                                id: 0,
                                content: (
                                  <div className="flex items-center">
                                    <div>
                                      {
                                        localizationCaptions[
                                          LocalizationKey.Chat
                                        ]
                                      }
                                    </div>
                                    {!!unreadChatMessages && (
                                      <>
                                        <Gap sizeRem={0.5} horizontal />
                                        <UnreadChatMessagesCounter
                                          value={unreadChatMessages}
                                        />
                                      </>
                                    )}
                                  </div>
                                ),
                              },
                              {
                                id: 1,
                                content: (
                                  <div className="flex items-center">
                                    <div>
                                      {
                                        localizationCaptions[
                                          LocalizationKey.RoomParticipants
                                        ]
                                      }
                                    </div>
                                    <Gap sizeRem={0.5} horizontal />
                                    <div>{peers.length + 1}</div>
                                  </div>
                                ),
                              },
                            ]}
                            activeIndex={messagesChatEnabled ? 0 : 1}
                            activeVariant="invertedActive"
                            nonActiveVariant="invertedAlternative"
                            onClick={handleSwitchMessagesChat}
                          />
                          <Gap sizeRem={0.875} horizontal />
                          <Button
                            variant="invertedAlternative"
                            className="min-w-[0rem] w-[2.5rem] h-[2.5rem] !p-[0rem]"
                          >
                            <Icon size="s" name={IconNames.Grid} />
                          </Button>
                        </div>
                      )}
                      {aiRoom && (
                        <div className="flex">
                          <div className="flex items-center bg-wrap px-[1rem] h-[2.5rem] rounded-[6.25rem]">
                            <Typography size="m" error>
                              <div className="flex">
                                <Icon
                                  inheritFontSize
                                  name={IconNames.RadioButtonOn}
                                />
                              </div>
                            </Typography>
                            <Gap sizeRem={0.25} horizontal />
                            <Typography size="m">
                              {
                                localizationCaptions[
                                  LocalizationKey.MeetingBeingRecorded
                                ]
                              }
                            </Typography>
                          </div>
                          {device === 'Desktop' && (
                            <Gap sizeRem={10.375} horizontal />
                          )}
                        </div>
                      )}
                    </div>
                    <div className="room-page-main-content">
                      <div className="room-columns">
                        {errorRoomState && (
                          <div>
                            {
                              localizationCaptions[
                                LocalizationKey.ErrorLoadingRoomState
                              ]
                            }
                            ...
                          </div>
                        )}
                        {!aiRoom && (currentUserExpert || viewerMode) && (
                          <RoomQuestionPanel
                            roomQuestionsLoading={roomQuestionsLoading}
                            roomQuestions={
                              roomQuestions?.sort(sortRoomQuestion) || []
                            }
                            initialQuestion={currentQuestion}
                          />
                        )}
                        {aiRoom ? (
                          <VideoChatAi
                            messagesChatEnabled={messagesChatEnabled}
                            // ScreenShare
                            // screenStream={screenStream}
                            roomQuestionsLoading={roomQuestionsLoading}
                            roomQuestions={
                              roomQuestions?.sort(sortRoomQuestion) || []
                            }
                            initialQuestion={currentQuestion}
                          />
                        ) : (
                          <VideoChat
                            messagesChatEnabled={messagesChatEnabled}
                            recognitionNotSupported={recognitionNotSupported}
                            recognitionEnabled={recognitionEnabled}
                            reactionsVisible={reactionsVisible}
                            currentUserExpert={currentUserExpert}
                            loadingRoomStartReview={loadingRoomStartReview}
                            errorRoomStartReview={errorRoomStartReview}
                            // ScreenShare
                            // screenStream={screenStream}
                            setRecognitionEnabled={setRecognitionEnabled}
                            handleInvitationsOpen={handleInvitationsOpen}
                            handleStartReviewRoom={handleStartReviewRoom}
                            handleSettingsOpen={handleSettingsOpen}
                            handleLeaveRoom={handleLeaveRoom}
                          />
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              </ProcessWrapper>
            </>
          )}
        </MainContentWrapper>
      </UserStreamsContext.Provider>
    </RoomContext.Provider>
  );
};
