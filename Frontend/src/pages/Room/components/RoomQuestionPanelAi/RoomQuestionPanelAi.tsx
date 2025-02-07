import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
import { Canvas } from '@react-three/fiber';
import { EffectComposer, FXAA } from '@react-three/postprocessing';
import {
  Room,
  RoomQuestion,
  RoomQuestionEvaluation as RoomQuestionEvaluationType,
} from '../../../../types/room';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import {
  ChangeActiveQuestionBody,
  GetRoomQuestionEvaluationParams,
  MergeRoomQuestionEvaluationBody,
  roomQuestionApiDeclaration,
  roomQuestionEvaluationApiDeclaration,
  roomsApiDeclaration,
} from '../../../../apiDeclarations';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Gap } from '../../../../components/Gap/Gap';
import { RoomQuestionEvaluationValue } from '../RoomQuestionEvaluation/RoomQuestionEvaluation';
import { Loader } from '../../../../components/Loader/Loader';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { Button } from '../../../../components/Button/Button';
import { RoomContext } from '../../context/RoomContext';
import {
  useVoiceRecognitionAccum,
  VoiceRecognitionCommand,
} from '../../hooks/useVoiceRecognitionAccum';
import { AiAssistant, AiAssistantScriptName } from '../AiAssistant/AiAssistant';
import { ReviewUserOpinion } from '../../../RoomAnaytics/components/ReviewUserOpinion/ReviewUserOpinion';
import { AnalyticsUserReview } from '../../../../types/analytics';
import { useAiAnswerSource } from '../../hooks/useAiAnswerSource';
import { AuthContext } from '../../../../context/AuthContext';

const notFoundCode = 404;
const aiAssistantGoodRate = 6;

const aiExpertId = 'aiExpertId';
const allUsersWithAiExpert = new Map<string, AnalyticsUserReview>();
allUsersWithAiExpert.set(aiExpertId, {
  comment: '',
  nickname: 'AI Expert',
  participantType: 'Expert',
  userId: aiExpertId,
  avatar: '/aiLogo192.png',
});

interface Question {
  id: string;
  value: string;
  tags: string[];
  nextQuestions: Record<string, number>;
}

const questions: Question[] = [
  {
    id: '0193cfe7-4053-75d1-95f8-8b73b837de24',
    value: 'Что из себя представляет контекст выполнения?',
    tags: ['контекст', 'this', 'зис'],
    nextQuestions: { '0193cfe9-4bce-76ce-9682-9ccc93d49a57': 1.0 },
  },
  {
    id: '0193cfe9-4bce-76ce-9682-9ccc93d49a57',
    value: 'Как переопределить контекст у функции?',
    tags: ['контекст', 'функция'],
    nextQuestions: {},
  },
  {
    id: '0193d011-2df2-75cd-936e-69695355a604',
    value: 'Что такое «Лексическое окружение»?',
    tags: ['лексическое', 'замыкание', 'замыкания'],
    nextQuestions: { '0193cfdb-28a7-7c7d-8319-0664d689991c': 1.0 },
  },
  {
    id: '0193cfdb-28a7-7c7d-8319-0664d689991c',
    value: 'Что такое «Замыкание»?',
    tags: ['лексическое', 'замыкание', 'замыкания'],
    nextQuestions: { '0193cfe7-4053-75d1-95f8-8b73b837de24': 1.0 },
  },
  {
    id: '0193ee7d-4e7d-7e57-b337-2d567a563722',
    value: 'Какие есть типы в JS?',
    tags: ['типы', 'типизированный'],
    nextQuestions: {
      '0193ee81-0be8-73ec-ad29-a4bf4616bb00': 1.0,
      '0193ee8a-2bca-7339-a55f-3665d29f03aa': 0.6,
    },
  },
  {
    id: '0193ee81-0be8-73ec-ad29-a4bf4616bb00',
    value: 'В чём различие null и undefined?',
    tags: ['нал', 'now', 'null', 'undefine'],
    nextQuestions: { '0193ee8a-2bca-7339-a55f-3665d29f03aa': 1.0 },
  },
  {
    id: '0193ee8a-2bca-7339-a55f-3665d29f03aa',
    value: 'Что такое объект в JS?',
    tags: ['объект', 'объекты', 'объектов', 'object'],
    nextQuestions: { '0193ee8c-775a-7130-9991-8530e0e1f3b3': 1.0 },
  },
  {
    id: '0193ee8c-775a-7130-9991-8530e0e1f3b3',
    value: 'Какого типа могут быть ключи у объекта?',
    tags: ['объект', 'object', 'ключи', 'ключ'],
    nextQuestions: { '0193d011-2df2-75cd-936e-69695355a604': 1.0 },
  },
  {
    id: '0194df5d-a8fe-760d-a28e-c7c56fc51dfe',
    value: 'Как в JS происходит управление памятью?',
    tags: ['памятью', 'память', 'стек', 'куча', 'хип', 'heap', 'stack'],
    nextQuestions: { '0193ee7e-ce18-79ef-b7e2-a93edab25b94': 1.0 },
  },
  {
    id: '0193ee7e-ce18-79ef-b7e2-a93edab25b94',
    value: 'Что такое сборщик мусора?',
    tags: ['сборщик', 'мусора', 'гц', 'сборка', 'гербович коллектор'],
    nextQuestions: { '0193ee7f-b26b-70e3-a7c8-5add6a256ed3': 1.0 },
  },
  {
    id: '0193ee7f-b26b-70e3-a7c8-5add6a256ed3',
    value: 'Какие есть алгоритмы сборки мусора?',
    tags: ['сборщик', 'гц', 'сборка', 'гербович коллектор'],
    nextQuestions: {},
  },
];

const findQuestionById = (id: string) =>
  questions.find((question) => question.id === id);

const normalizeWords = (words: string[]) =>
  words.map((word) => word.trim().toLowerCase());

const findQuestionsWithTag = (tag: string) =>
  questions.filter((question) => question.tags.indexOf(tag) !== -1);

const getRandomQuestion = () =>
  questions[Math.floor(Math.random() * questions.length)];

const getRandomQuestionWithNextQuestions = () => {
  for (let i = 30; i--; ) {
    const randomQuestion = getRandomQuestion();
    if (Object.keys(randomQuestion.nextQuestions).length !== 0) {
      return randomQuestion;
    }
  }
  return getRandomQuestion();
};

export interface RoomQuestionPanelAiProps {
  roomQuestionsLoading: boolean;
  roomQuestions: RoomQuestion[];
  initialQuestion?: RoomQuestion;
}

export const RoomQuestionPanelAi: FunctionComponent<
  RoomQuestionPanelAiProps
> = ({ roomQuestionsLoading, roomQuestions, initialQuestion }) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const {
    room,
    roomParticipant,
    lastVoiceRecognition,
    aiAssistantScript,
    setAiAssistantCurrentScript,
    setRecognitionEnabled,
  } = useContext(RoomContext);
  const {
    recognitionAccum,
    recognitionCommand,
    resetVoiceRecognitionAccum,
    addVoiceRecognitionAccumTranscript,
  } = useVoiceRecognitionAccum();
  const readOnly = roomParticipant?.userType !== 'Expert';
  const [roomQuestionEvaluation, setRoomQuestionEvaluation] =
    useState<RoomQuestionEvaluationValue | null>(null);
  const [copilotAnswerOpen, setCopilotAnswerOpen] = useState(false);
  const startedByVoiceRef = useRef(false);
  const [nextQuestionsMap, setNextQuestionsMap] = useState<
    Record<string, number>
  >({});
  const nextQuestions = Object.entries(nextQuestionsMap)
    .sort(([, factor1], [, factor2]) => {
      if (factor1 < factor2) {
        return 1;
      }
      if (factor1 > factor2) {
        return -1;
      }
      return 0;
    })
    .map(([questionId]) => findQuestionById(questionId))
    .filter(Boolean);

  const {
    apiMethodState: apiSendActiveQuestionState,
    fetchData: sendRoomActiveQuestion,
  } = useApiMethod<unknown, ChangeActiveQuestionBody>(
    roomQuestionApiDeclaration.changeActiveQuestion,
  );
  const {
    process: {
      loading: loadingRoomActiveQuestion,
      error: errorRoomActiveQuestion,
    },
  } = apiSendActiveQuestionState;

  const {
    apiMethodState: apiRoomStartReviewMethodState,
    fetchData: fetchRoomStartReview,
  } = useApiMethod<unknown, Room['id']>(roomsApiDeclaration.startReview);
  const {
    process: { loading: loadingRoomStartReview, error: errorRoomStartReview },
  } = apiRoomStartReviewMethodState;

  const {
    apiMethodState: apiRoomQuestionEvaluationState,
    fetchData: getRoomQuestionEvaluation,
  } = useApiMethod<RoomQuestionEvaluationType, GetRoomQuestionEvaluationParams>(
    roomQuestionEvaluationApiDeclaration.get,
  );
  const {
    data: loadedRoomQuestionEvaluation,
    process: {
      error: errorRoomQuestionEvaluation,
      code: responseCodeRoomQuestionEvaluation,
    },
  } = apiRoomQuestionEvaluationState;

  const {
    apiMethodState: apiMergeRoomQuestionEvaluationState,
    fetchData: mergeRoomQuestionEvaluation,
  } = useApiMethod<RoomQuestionEvaluationType, MergeRoomQuestionEvaluationBody>(
    roomQuestionEvaluationApiDeclaration.merge,
  );
  const {
    data: mergedRoomQuestionEvaluation,
    process: {
      loading: loadingMergeRoomQuestionEvaluation,
      error: errorMergeRoomQuestionEvaluation,
    },
  } = apiMergeRoomQuestionEvaluationState;

  const getRoomQuestionEvaluationError =
    responseCodeRoomQuestionEvaluation !== notFoundCode
      ? errorRoomQuestionEvaluation
      : null;
  const totalErrorRoomQuestionEvaluation =
    errorMergeRoomQuestionEvaluation || getRoomQuestionEvaluationError;

  const { aiAnswerCompleted, aiAnswerLoading, lastValidAiAnswer } =
    useAiAnswerSource({
      enabled: copilotAnswerOpen,
      answer: recognitionAccum,
      conversationId: `${room?.id}${initialQuestion?.id}${auth?.id}`,
      question: initialQuestion?.value || '',
      questionId: initialQuestion?.id || '',
      theme: room?.category?.name || '',
      userId: auth?.id || '',
    });

  const closedQuestions = roomQuestions.filter(
    (roomQuestion) => roomQuestion.state === 'Closed',
  );
  const openQuestions = roomQuestions.filter(
    (roomQuestion) => roomQuestion.state === 'Open',
  );
  const readyToReview =
    closedQuestions.length > 4 || openQuestions.length === 0;
  const nextQuestionButtonLoading =
    !mergedRoomQuestionEvaluation ||
    loadingMergeRoomQuestionEvaluation ||
    !aiAnswerCompleted;
  const letsStartDescription = localizationCaptions[
    LocalizationKey.LetsBeginDescription
  ].replace(
    '{LetsStartCommand}',
    localizationCaptions[LocalizationKey.LetsBeginCommand],
  );
  const rateMeDescription = localizationCaptions[
    LocalizationKey.RateMeDescription
  ].replace(
    '{RateMeCommand}',
    localizationCaptions[LocalizationKey.RateMeCommand],
  );

  useEffect(() => {
    setRecognitionEnabled(!copilotAnswerOpen);
  }, [copilotAnswerOpen, setRecognitionEnabled]);

  useEffect(() => {
    if (initialQuestion) {
      const currQuestion = findQuestionById(initialQuestion.id);
      if (!currQuestion) {
        return;
      }
      if (Object.keys(currQuestion.nextQuestions).length === 0) {
        const randWithNextQuestions = getRandomQuestionWithNextQuestions();
        setNextQuestionsMap(randWithNextQuestions.nextQuestions || {});
        return;
      }
      setNextQuestionsMap(currQuestion?.nextQuestions || {});
      return;
    }
    const randomQuestion = getRandomQuestionWithNextQuestions();
    setNextQuestionsMap(randomQuestion?.nextQuestions || {});
  }, [initialQuestion]);

  const addNextQuestionContext = useCallback((message: string) => {
    const normalizedWords = normalizeWords(message.trim().split(' '));
    const questionsWithTags: Question[] = [];
    for (const word of normalizedWords) {
      questionsWithTags.push(...findQuestionsWithTag(word));
    }
    setNextQuestionsMap((oldNextQuestionsMap) => {
      const clone = { ...oldNextQuestionsMap };
      questionsWithTags.forEach((questionWithTags) => {
        if (!clone[questionWithTags.id]) {
          clone[questionWithTags.id] = 0.0;
        }
        clone[questionWithTags.id] += 0.3;
      });
      return clone;
    });
  }, []);

  useEffect(() => {
    if (!lastVoiceRecognition) {
      return;
    }
    const message = lastVoiceRecognition;
    addNextQuestionContext(message);
  }, [lastVoiceRecognition, addNextQuestionContext]);

  useEffect(() => {
    if (room?.status === 'New') {
      addNextQuestionContext(room.name);
    }
  }, [room, addNextQuestionContext]);

  useEffect(() => {
    if (!lastVoiceRecognition) {
      return;
    }
    addVoiceRecognitionAccumTranscript(lastVoiceRecognition);
  }, [lastVoiceRecognition, addVoiceRecognitionAccumTranscript]);

  useEffect(() => {
    if (!initialQuestion?.id) {
      return;
    }
    resetVoiceRecognitionAccum();
  }, [initialQuestion?.id, resetVoiceRecognitionAccum]);

  const handleCopilotAnswerOpen = useCallback(() => {
    setCopilotAnswerOpen(true);
  }, []);

  const handleCopilotAnswerClose = useCallback(() => {
    setCopilotAnswerOpen(false);
  }, []);

  useEffect(() => {
    if (recognitionCommand !== VoiceRecognitionCommand.RateMe) {
      return;
    }
    handleCopilotAnswerOpen();
  }, [recognitionCommand, handleCopilotAnswerOpen]);

  useEffect(() => {
    if (!aiAnswerCompleted || !lastValidAiAnswer) {
      return;
    }
    setRoomQuestionEvaluation({
      mark: Math.round(lastValidAiAnswer?.score),
      review: lastValidAiAnswer?.reason,
    });
  }, [aiAnswerCompleted, lastValidAiAnswer]);

  useEffect(() => {
    if (!room) {
      return;
    }
    if (room.status === 'New') {
      setAiAssistantCurrentScript(AiAssistantScriptName.Welcome);
    }
  }, [room, setAiAssistantCurrentScript]);

  useEffect(() => {
    if (!initialQuestion?.id) {
      return;
    }
    setAiAssistantCurrentScript(AiAssistantScriptName.PleaseAnswer);
  }, [initialQuestion?.id, setAiAssistantCurrentScript]);

  useEffect(() => {
    if (!aiAnswerCompleted) {
      return;
    }
    if (lastValidAiAnswer?.score >= aiAssistantGoodRate) {
      setAiAssistantCurrentScript(AiAssistantScriptName.GoodAnswer);
    } else {
      setAiAssistantCurrentScript(AiAssistantScriptName.NeedTrain);
    }
  }, [aiAnswerCompleted, lastValidAiAnswer, setAiAssistantCurrentScript]);

  useEffect(() => {
    if (readOnly || !room || !initialQuestion) {
      return;
    }
    getRoomQuestionEvaluation({
      questionId: initialQuestion.id,
      roomId: room.id,
    });
  }, [readOnly, room, initialQuestion, getRoomQuestionEvaluation]);

  useEffect(() => {
    if (!roomQuestionEvaluation) {
      return;
    }
    const activeQuestion = roomQuestions?.find(
      (question) => question.state === 'Active',
    );
    const roomId = room?.id;
    if (!activeQuestion || !roomId) {
      return;
    }
    mergeRoomQuestionEvaluation({
      ...roomQuestionEvaluation,
      questionId: activeQuestion.id,
      roomId: roomId,
      review: roomQuestionEvaluation.review || '',
      mark: roomQuestionEvaluation.mark || null,
    });
  }, [
    roomQuestionEvaluation,
    room?.id,
    roomQuestions,
    mergeRoomQuestionEvaluation,
  ]);

  useEffect(() => {
    if (!loadedRoomQuestionEvaluation) {
      return;
    }
    setRoomQuestionEvaluation(loadedRoomQuestionEvaluation);
  }, [loadedRoomQuestionEvaluation]);

  useEffect(() => {
    if (responseCodeRoomQuestionEvaluation !== notFoundCode) {
      return;
    }
    setRoomQuestionEvaluation({
      mark: null,
      review: '',
    });
  }, [responseCodeRoomQuestionEvaluation]);

  const handleNextQuestion = useCallback(() => {
    if (!room) {
      throw new Error('handleNextQuestion Room not found.');
    }
    const nextQId = nextQuestions[0]?.id;
    if (!nextQId) {
      console.warn('handleNextQuestion empty nextQuestion');
      return;
    }
    handleCopilotAnswerClose();
    resetVoiceRecognitionAccum();
    sendRoomActiveQuestion({
      roomId: room.id,
      questionId: nextQId,
    });
  }, [
    room,
    nextQuestions,
    handleCopilotAnswerClose,
    resetVoiceRecognitionAccum,
    sendRoomActiveQuestion,
  ]);

  useEffect(() => {
    if (
      startedByVoiceRef.current ||
      recognitionCommand !== VoiceRecognitionCommand.LetsStart ||
      initialQuestion ||
      !room
    ) {
      return;
    }
    if (room.status !== 'New') {
      return;
    }
    startedByVoiceRef.current = true;
    handleNextQuestion();
  }, [recognitionCommand, initialQuestion, room, handleNextQuestion]);

  const handleStartReviewRoom = useCallback(() => {
    if (!room?.id) {
      throw new Error('Room id not found');
    }
    fetchRoomStartReview(room.id);
  }, [room?.id, fetchRoomStartReview]);

  const firstLineCaption = initialQuestion
    ? initialQuestion.value
    : localizationCaptions[LocalizationKey.WaitingInterviewStart];
  const secondLineCaption = initialQuestion
    ? rateMeDescription
    : letsStartDescription;
  const loadingTotal =
    loadingRoomStartReview ||
    roomQuestionsLoading ||
    loadingRoomActiveQuestion ||
    aiAnswerLoading;

  return (
    <>
      <div className="flex flex-col z-50">
        <Typography size="xxxl" bold>
          {firstLineCaption}
        </Typography>
        <Gap sizeRem={0.5} />
        <Typography size="m">{secondLineCaption}</Typography>
        {errorRoomActiveQuestion && (
          <>
            <Gap sizeRem={1} />
            <Typography size="m" error>
              {localizationCaptions[LocalizationKey.ErrorSendingActiveQuestion]}
            </Typography>
          </>
        )}
      </div>

      {copilotAnswerOpen && (
        <div className="absolute w-full h-full flex items-center justify-center">
          <div className="flex flex-col px-1.5 z-10">
            <ReviewUserOpinion
              user={{
                id: aiExpertId,
                evaluation: {
                  mark: parseFloat(lastValidAiAnswer?.score) || null,
                  review: lastValidAiAnswer?.reason,
                  expected: lastValidAiAnswer?.expected,
                  recommendation: lastValidAiAnswer?.recommendation,
                },
              }}
              allUsers={allUsersWithAiExpert}
            />
            <div>
              <Gap sizeRem={1.75} />
              {totalErrorRoomQuestionEvaluation && (
                <Typography size="m" error>
                  {totalErrorRoomQuestionEvaluation}
                </Typography>
              )}
              {errorRoomStartReview && (
                <Typography size="m" error>
                  {errorRoomStartReview}
                </Typography>
              )}
              <div className="flex justify-center w-full">
                <Button
                  className="flex items-center"
                  variant="active"
                  disabled={nextQuestionButtonLoading}
                  onClick={
                    readyToReview ? handleStartReviewRoom : handleNextQuestion
                  }
                >
                  {nextQuestionButtonLoading ? (
                    <Loader />
                  ) : (
                    <>
                      <span>
                        {
                          localizationCaptions[
                            readyToReview
                              ? LocalizationKey.StartReviewRoom
                              : LocalizationKey.NextRoomQuestion
                          ]
                        }
                      </span>
                      <Gap sizeRem={0.5} horizontal />
                      <Icon
                        name={
                          readyToReview
                            ? IconNames.Stop
                            : IconNames.ChevronForward
                        }
                      />
                    </>
                  )}
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}

      <div
        className="absolute w-full h-full z-0"
        style={{ opacity: copilotAnswerOpen ? 0.05 : 1.0 }}
      >
        <Canvas shadows camera={{ position: [0, 0.5, 6.5], fov: 38 }}>
          <EffectComposer>
            <FXAA />
          </EffectComposer>
          <AiAssistant
            loading={loadingTotal}
            currentScript={aiAssistantScript}
          />
        </Canvas>
      </div>
    </>
  );
};
