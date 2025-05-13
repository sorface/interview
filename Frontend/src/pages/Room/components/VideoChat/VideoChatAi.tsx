import React, {
  FunctionComponent,
  useContext,
  useEffect,
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
import { IconNames } from '../../../../constants';
import { Gap } from '../../../../components/Gap/Gap';
import { Icon } from '../Icon/Icon';
import { RoomContext } from '../../context/RoomContext';
import { RoomQuestionPanelAi } from '../RoomQuestionPanelAi/RoomQuestionPanelAi';
import { RoomQuestion } from '../../../../types/room';
import { sortRoomQuestion } from '../../../../utils/sortRoomQestions';
import { Theme, ThemeContext } from '../../../../context/ThemeContext';
import { Button } from '../../../../components/Button/Button';
import { ThemeSwitchMini } from '../../../../components/ThemeSwitchMini/ThemeSwitchMini';
import { LangSwitch } from '../../../../components/LangSwitch/LangSwitch';
import { QuestionsProgress } from './QuestionsProgress';

const transcriptsMaxLength = 100;
const viewerOrder = 666;

interface VideoChatAiProps {
  messagesChatEnabled: boolean;
  roomQuestionsLoading: boolean;
  roomQuestions: RoomQuestion[];
  initialQuestion?: RoomQuestion;
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
  roomQuestionsLoading,
  roomQuestions,
  initialQuestion,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const { themeInUi } = useContext(ThemeContext);
  const {
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
    apiMethodState: apiRoomEventsSearchState,
    fetchData: fetchRoomEventsSearch,
  } = useApiMethod<EventsSearch, RoomIdParam>(roomsApiDeclaration.eventsSearch);
  const { data: roomEventsSearch } = apiRoomEventsSearchState;
  const [textMessages, setTextMessages] = useState<Transcript[]>([]);
  const { activeReactions } = useReactionsStatus({
    lastWsMessageParsed,
  });
  const closedQuestions = roomQuestions.filter((q) => q.state === 'Closed');
  const questionsProgress = ~~(
    (closedQuestions.length / roomQuestions.length) *
    100
  );

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

  const handleTextMessageSubmit = (message: string) => {
    sendWsMessage(
      JSON.stringify({
        Type: 'chat-message',
        Value: message,
      }),
    );
  };

  const handleRecognitionSwitch = () => {
    setRecognitionEnabled(!recognitionEnabled);
  };

  return (
    <>
      <div className="flex-1 flex justify-center z-10">
        <div className="w-full flex flex-col relative rounded-[1.125rem]">
          <RoomQuestionPanelAi
            questionWithCode={codeEditorEnabled}
            roomQuestionsLoading={roomQuestionsLoading}
            roomQuestions={roomQuestions?.sort(sortRoomQuestion) || []}
            initialQuestion={initialQuestion}
          >
            <div
              className="absolute flex flex-col items-end justify-center"
              style={{
                right: '-106px',
                bottom: '-53px',
              }}
            >
              {themeInUi === Theme.Light && (
                <div
                  style={{
                    position: 'absolute',
                    width: '931px',
                    height: '547px',
                    top: '87px',
                    left: '-124px',
                    background: 'rgba(128, 112, 196, 0.23)',
                    filter: 'blur(79.1px)',
                    transform: 'rotate(40.92deg)',
                    pointerEvents: 'none',
                  }}
                ></div>
              )}
            </div>
          </RoomQuestionPanelAi>
          <Gap sizeRem={1.5} />
          <div className="flex h-[2.375rem]">
            <Gap sizeRem={3.375} horizontal />
            <LangSwitch elementType="button" />
            <Gap sizeRem={0.5} horizontal />
            <ThemeSwitchMini variant="button" />
            <Gap sizeRem={1.75} horizontal />
            <QuestionsProgress value={questionsProgress} />
            <Button
              variant="invertedAlternative"
              className="min-w-[0rem] w-[2.375rem] h-[2.375rem] !p-[0rem] ml-auto"
              onClick={handleRecognitionSwitch}
            >
              <Icon
                size="s"
                name={recognitionEnabled ? IconNames.MicOn : IconNames.MicOff}
              />
            </Button>
            <Gap sizeRem={10.375} horizontal />
          </div>
          <Gap sizeRem={1.875} />
        </div>
      </div>

      <div className="absolute videochat-field overflow-auto right-[1rem]">
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
          className={`absolute top-[0rem] h-full bg-wrap w-full ${messagesChatEnabled ? 'visible' : 'invisible'} z-1`}
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
