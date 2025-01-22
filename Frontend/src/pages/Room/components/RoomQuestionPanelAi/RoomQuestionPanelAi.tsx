import {
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
  copilotApiDeclaration,
  CopilotEvaluateAnswerBody,
  CopilotEvaluateAnswerResponse,
  GetRoomQuestionEvaluationParams,
  MergeRoomQuestionEvaluationBody,
  roomQuestionApiDeclaration,
  roomQuestionEvaluationApiDeclaration,
  roomsApiDeclaration,
} from '../../../../apiDeclarations';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Gap } from '../../../../components/Gap/Gap';
import {
  RoomQuestionEvaluationValue,
} from '../RoomQuestionEvaluation/RoomQuestionEvaluation';
import { Loader } from '../../../../components/Loader/Loader';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { Button } from '../../../../components/Button/Button';
import { RoomContext } from '../../context/RoomContext';
import { useVoiceRecognitionAccum, VoiceRecognitionCommand } from '../../hooks/useVoiceRecognitionAccum';
import { AiAssistant, AiAssistantScriptName } from '../AiAssistant/AiAssistant';
import { ReviewUserOpinion } from '../../../RoomAnaytics/components/ReviewUserOpinion/ReviewUserOpinion';
import { AnalyticsUserReview } from '../../../../types/analytics';

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
    id: '55ec12b1-239b-451e-9464-6d3845f4f133',
    value: 'Что из себя представляет контекст выполнения?',
    tags: ['контекст', 'this', 'зис'],
    nextQuestions: { '8c732b6a-7b4f-48b8-8533-30b83220facb': 1.0 },
  },
  {
    id: '8c732b6a-7b4f-48b8-8533-30b83220facb',
    value: 'Как переопределить контекст у функции?',
    tags: ['контекст', 'функция'],
    nextQuestions: {},
  },
  {
    id: 'a4fb513e-16dd-4cba-b504-d98fd98abe27',
    value: 'Что такое «Лексическое окружение»?',
    tags: ['лексическое', 'замыкание', 'замыкания'],
    nextQuestions: { '6915a9a3-6c36-491c-9588-19592843544d': 1.0 },
  },
  {
    id: '6915a9a3-6c36-491c-9588-19592843544d',
    value: 'Что такое «Замыкание»?',
    tags: ['лексическое', 'замыкание', 'замыкания'],
    nextQuestions: { '55ec12b1-239b-451e-9464-6d3845f4f133': 1.0 },
  },
  {
    id: 'e0c1c923-d6a2-41b1-a6b6-b5e82e8e4de8',
    value: 'Какие есть типы в JS?',
    tags: ['типы', 'типизированный'],
    nextQuestions: { '36a63b67-0e6a-4130-83be-0fb1e7f64642': 1.0, 'd0d0cd70-5bb6-4a84-9e46-734af5b47697': 0.6 },
  },
  {
    id: '36a63b67-0e6a-4130-83be-0fb1e7f64642',
    value: 'В чём различие null и undefined?',
    tags: ['нал', 'now', 'null', 'undefine'],
    nextQuestions: { 'd0d0cd70-5bb6-4a84-9e46-734af5b47697': 1.0 },
  },
  {
    id: 'd0d0cd70-5bb6-4a84-9e46-734af5b47697',
    value: 'Что такое объект в JS?',
    tags: ['объект', 'объекты', 'объектов', 'object',],
    nextQuestions: { '7231b74e-60e4-4aec-bc0b-afc4ff4f2659': 1.0 },
  },
  {
    id: '7231b74e-60e4-4aec-bc0b-afc4ff4f2659',
    value: 'Какого типа могут быть ключи у объекта?',
    tags: ['объект', 'object', 'ключи', 'ключ'],
    nextQuestions: { 'a4fb513e-16dd-4cba-b504-d98fd98abe27': 1.0 },
  },
  {
    id: 'd3a87acd-57ba-4337-8ff6-31d64e20bca9',
    value: 'Что такое JS?',
    tags: ['JS', 'javascript', 'gs', 'javascript'],
    nextQuestions: { 'e0c1c923-d6a2-41b1-a6b6-b5e82e8e4de8': 1.0 },
  },
  {
    id: '1a325851-f68f-4be6-8cfe-6ecd9c3c5486',
    value: 'Что такое TypeScript?',
    tags: ['typescript', 'ts', 'типизация'],
    nextQuestions: { '3b0eab35-32f6-4505-a705-830c64fc7477': 1.0 },
  },
  {
    id: '3b0eab35-32f6-4505-a705-830c64fc7477',
    value: 'Как задать тип объекту?',
    tags: ['интерфейс', 'тип'],
    nextQuestions: {},
  },
  {
    id: '464f511e-1d76-4d09-9ad2-69b8a22e9882',
    value: 'Как в JS происходит управление памятью?',
    tags: ['памятью', 'память', 'стек', 'куча', 'хип', 'heap', 'stack'],
    nextQuestions: { 'bff01b27-0a9b-4b14-a88b-59dd2e3ac108': 1.0 },
  },
  {
    id: 'bff01b27-0a9b-4b14-a88b-59dd2e3ac108',
    value: 'Что такое сборщик мусора?',
    tags: ['сборщик', 'мусора', 'гц', 'сборка', 'гербович коллектор'],
    nextQuestions: { '3733a026-e0f0-4fd5-9e50-4c4f2a21ace9': 1.0 },
  },
  {
    id: '3733a026-e0f0-4fd5-9e50-4c4f2a21ace9',
    value: 'Какие есть алгоритмы сборки мусора?',
    tags: ['сборщик', 'гц', 'сборка', 'гербович коллектор'],
    nextQuestions: {},
  },
];

const findQuestionById = (id: string) =>
  questions.find(question => question.id === id);

const normalizeWords = (words: string[]) =>
  words.map(word => word.trim().toLowerCase());

const findQuestionsWithTag = (tag: string) =>
  questions.filter(question => question.tags.indexOf(tag) !== -1);

export interface RoomQuestionPanelAiProps {
  roomQuestionsLoading: boolean;
  roomQuestions: RoomQuestion[];
  initialQuestion?: RoomQuestion;
}

export const RoomQuestionPanelAi: FunctionComponent<RoomQuestionPanelAiProps> = ({
  roomQuestionsLoading,
  roomQuestions,
  initialQuestion,
}) => {
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
  const [nextQuestionsMap, setNextQuestionsMap] = useState<Record<string, number>>({});
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

  const {
    apiMethodState: apiEvaluateAnswerState,
    fetchData: fetchEvaluateAnswer,
  } = useApiMethod<CopilotEvaluateAnswerResponse, CopilotEvaluateAnswerBody>(
    copilotApiDeclaration.evaluateAnswer,
  );
  const {
    process: { loading: evaluateAnswerLoading, error: evaluateAnswerError },
    data: evaluateAnswerData,
  } = apiEvaluateAnswerState;

  const getRoomQuestionEvaluationError =
    responseCodeRoomQuestionEvaluation !== notFoundCode
      ? errorRoomQuestionEvaluation
      : null;
  const totalErrorRoomQuestionEvaluation =
    errorMergeRoomQuestionEvaluation || getRoomQuestionEvaluationError;

  const closedQuestions = roomQuestions.filter(
    (roomQuestion) => roomQuestion.state === 'Closed',
  );
  const openQuestions = roomQuestions.filter(
    (roomQuestion) => roomQuestion.state === 'Open',
  );
  const readyToReview = closedQuestions.length > 4 || openQuestions.length === 0;
  const nextQuestionButtonLoading =
    (!mergedRoomQuestionEvaluation || loadingMergeRoomQuestionEvaluation) ||
    (!evaluateAnswerData || evaluateAnswerLoading);
  const letsStartDescription = localizationCaptions[LocalizationKey.LetsBeginDescription]
    .replace('{LetsStartCommand}', localizationCaptions[LocalizationKey.LetsBeginCommand]);
  const rateMeDescription = localizationCaptions[LocalizationKey.RateMeDescription]
    .replace('{RateMeCommand}', localizationCaptions[LocalizationKey.RateMeCommand]);

  useEffect(() => {
    setRecognitionEnabled(!copilotAnswerOpen);
  }, [copilotAnswerOpen, setRecognitionEnabled]);

  useEffect(() => {
    if (!initialQuestion) {
      return;
    }
    const currQ = findQuestionById(initialQuestion.id);
    if (!currQ) {
      console.warn('no next questions');
    }
    setNextQuestionsMap(currQ?.nextQuestions || {});
  }, [initialQuestion]);

  const addNextQuestionContext = useCallback((message: string) => {
    const normalizedWords = normalizeWords(message.trim().split(' '));
    const questionsWithTags: Question[] = [];
    for (let word of normalizedWords) {
      questionsWithTags.push(...findQuestionsWithTag(word));
    }
    setNextQuestionsMap((oldNextQuestionsMap) => {
      const clone = { ...oldNextQuestionsMap };
      questionsWithTags.forEach(questionWithTags => {

        if (!clone[questionWithTags.id]) {
          clone[questionWithTags.id] = 0.0;
        }
        clone[questionWithTags.id] += 0.3;
      })
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

  const handleRateMe = useCallback(() => {
    const question = initialQuestion?.value;
    if (!question) {
      return;
    }
    fetchEvaluateAnswer({
      question,
      transcript: recognitionAccum,
    });
  }, [initialQuestion?.value, recognitionAccum, fetchEvaluateAnswer]);

  useEffect(() => {
    if (!evaluateAnswerData) {
      return;
    }
    handleCopilotAnswerOpen();
  }, [evaluateAnswerData, handleCopilotAnswerOpen]);

  useEffect(() => {
    if (recognitionCommand !== VoiceRecognitionCommand.RateMe) {
      return;
    }
    handleRateMe();
  }, [recognitionCommand, handleRateMe]);

  useEffect(() => {
    if (!evaluateAnswerData) {
      return;
    }
    setRoomQuestionEvaluation({
      mark: evaluateAnswerData.mark,
      review: evaluateAnswerData.review,
    });
  }, [evaluateAnswerData]);

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
    if (!evaluateAnswerData) {
      return;
    }
    if (evaluateAnswerData.mark >= aiAssistantGoodRate) {
      setAiAssistantCurrentScript(AiAssistantScriptName.GoodAnswer);
    } else {
      setAiAssistantCurrentScript(AiAssistantScriptName.NeedTrain);
    }
  }, [evaluateAnswerData, setAiAssistantCurrentScript])

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
  }, [room, nextQuestions, handleCopilotAnswerClose, resetVoiceRecognitionAccum, sendRoomActiveQuestion]);

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

  const firstLineCaption = initialQuestion ?
    initialQuestion.value :
    localizationCaptions[LocalizationKey.WaitingInterviewStart];
  const secondLineCaption = initialQuestion ?
    rateMeDescription :
    letsStartDescription;
  const loadingTotal =
    loadingRoomStartReview ||
    roomQuestionsLoading ||
    loadingRoomActiveQuestion ||
    evaluateAnswerLoading;

  return (
    <>
      <div className='flex flex-col z-50'>
        <Typography size='xxxl' bold>{firstLineCaption}</Typography>
        <Gap sizeRem={0.5} />
        <Typography size='m'>{secondLineCaption}</Typography>
        {errorRoomActiveQuestion && (
          <>
            <Gap sizeRem={1} />
            <Typography size='m' error>
              {
                localizationCaptions[
                LocalizationKey.ErrorSendingActiveQuestion
                ]
              }
            </Typography>
          </>
        )}
      </div>

      {copilotAnswerOpen && !evaluateAnswerLoading && evaluateAnswerData && (
        <div className='absolute w-full h-full flex items-center justify-center'>
          <div className='flex flex-col px-1.5 z-10'>
            {evaluateAnswerLoading && <Loader />}
            {(!evaluateAnswerLoading && evaluateAnswerData) && (
              <ReviewUserOpinion
                user={{
                  id: aiExpertId,
                  evaluation: {
                    mark: evaluateAnswerData.mark,
                    review: evaluateAnswerData.review,
                  },
                }}
                allUsers={allUsersWithAiExpert}
              />
            )}
            <div>
              <Gap sizeRem={1.75} />
              {totalErrorRoomQuestionEvaluation && <Typography size='m' error>{totalErrorRoomQuestionEvaluation}</Typography>}
              {evaluateAnswerError && <Typography size='m' error>{evaluateAnswerError}</Typography>}
              {errorRoomStartReview && <Typography size='m' error>{errorRoomStartReview}</Typography>}
              <div className='flex justify-center w-full'>
                <Button
                  className="flex items-center"
                  variant="active"
                  onClick={readyToReview ? handleStartReviewRoom : handleNextQuestion}
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
                          readyToReview ? IconNames.Stop : IconNames.ChevronForward
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

      <div className='absolute w-full h-full z-0' style={{ opacity: copilotAnswerOpen ? 0.05 : 1.0 }}>
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
