import React, {
  FunctionComponent,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { Canvas } from '@react-three/fiber';
import {
  RoomQuestion,
  RoomQuestionEvaluation as RoomQuestionEvaluationType,
} from '../../../../types/room';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import {
  ChangeActiveQuestionBody,
  CompleteRoomReviewsBody,
  GetRoomQuestionEvaluationParams,
  MergeRoomQuestionEvaluationBody,
  roomQuestionApiDeclaration,
  roomQuestionEvaluationApiDeclaration,
  roomReviewApiDeclaration,
} from '../../../../apiDeclarations';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Gap } from '../../../../components/Gap/Gap';
import { RoomQuestionEvaluationValue } from '../RoomQuestionEvaluation/RoomQuestionEvaluation';
import { Loader } from '../../../../components/Loader/Loader';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../Icon/Icon';
import { aiExpertNickname, EventName, IconNames } from '../../../../constants';
import { Button } from '../../../../components/Button/Button';
import { RoomContext } from '../../context/RoomContext';
import {
  useVoiceRecognitionAccum,
  VoiceRecognitionCommand,
} from '../../hooks/useVoiceRecognitionAccum';
import { AiAssistant, AiAssistantScriptName } from '../AiAssistant/AiAssistant';
import { AnalyticsUserReview } from '../../../../types/analytics';
import { AiEndpoint, useAiAnswerSource } from '../../hooks/useAiAnswerSource';
import { AuthContext } from '../../../../context/AuthContext';
import { useThemedAiAvatar } from '../../../../hooks/useThemedAiAvatar';
import {
  QuestionsTree,
  QuestionsTreeNode,
} from '../../../../types/questionsTree';
import { RoomCodeEditor } from '../RoomCodeEditor/RoomCodeEditor';
import { CodeEditorLang } from '../../../../types/question';
import { AnyObject } from '../../../../types/anyObject';
import { Theme, ThemeContext } from '../../../../context/ThemeContext';
import { RoomTimerAi } from '../RoomTimerAi/RoomTimerAi';

const notFoundCode = 404;
const aiAssistantGoodRate = 6;
const questionAnswerTimer = 5 * 60;
const questionWithCodeAnswerTimer = 30 * 60;

const aiExpertId = 'aiExpertId';

const findNodeById = (nodes: QuestionsTreeNode[], id: string) =>
  nodes.find((node) => node.id === id);

const findNodeByQuestionId = (nodes: QuestionsTreeNode[], id: string) =>
  nodes.find((node) => node.question?.id === id);

const findNextNodes = (
  nodes: QuestionsTreeNode[],
  currentQuestionId: string,
): QuestionsTree['tree'] => {
  const currentNode = findNodeByQuestionId(nodes, currentQuestionId);
  if (!currentNode) {
    console.warn('no currentNode in getQuestions');
    return [];
  }
  const childNodes = nodes.filter(
    (node) => node.parentQuestionSubjectTreeId === currentNode.id,
  );
  return childNodes;
};

const getRandomNode = (nodes: QuestionsTreeNode[]) =>
  nodes[Math.floor(Math.random() * nodes.length)];

const getRandomQuestionWithExclude = (
  nodes: QuestionsTreeNode[],
  excludedQuestions: RoomQuestion[],
) => {
  for (let i = 777; i--; ) {
    const randomNode = getRandomNode(nodes);
    if (!randomNode.question) {
      continue;
    }
    const inExcludedQuestions = excludedQuestions.find(
      (eq) => eq.id === randomNode.question?.id,
    );
    if (inExcludedQuestions) {
      continue;
    }
    return randomNode.question;
  }
  return getRandomNode(nodes).question;
};

const findRandomNextNode = (
  nodes: QuestionsTreeNode[],
  excludedQuestions: RoomQuestion[],
  currentQuestionId: string,
) => {
  const nextNodes = findNextNodes(nodes, currentQuestionId);
  const nextNodesFiltered = nextNodes.filter(
    (node) =>
      !excludedQuestions.find((excluded) => node.question?.id === excluded.id),
  );
  if (nextNodesFiltered.length === 0) {
    return null;
  }
  return getRandomNode(nextNodesFiltered).question;
};

const visitNodesFromRoot = (
  nodes: QuestionsTreeNode[],
  currentNode: QuestionsTreeNode,
  excludedQuestions: RoomQuestion[],
): QuestionsTreeNode | null => {
  const childNodes = nodes.filter(
    (node) => node.parentQuestionSubjectTreeId === currentNode.id,
  );
  if (childNodes.length === 0) {
    return null;
  }
  const nextNodesFiltered = childNodes.filter(
    (node) =>
      !excludedQuestions.find((excluded) => node.question?.id === excluded.id),
  );
  if (nextNodesFiltered.length !== 0) {
    return getRandomNode(nextNodesFiltered);
  }
  const randomChild = getRandomNode(childNodes);
  return visitNodesFromRoot(nodes, randomChild, excludedQuestions);
};

const tryGetRandomNodeFromRoot = (
  tree: QuestionsTree,
  excludedQuestions: RoomQuestion[],
) => {
  const nodes = tree.tree;
  const rootNode = findNodeById(nodes, tree.rootQuestionSubjectTreeId);
  if (!rootNode) {
    console.warn('no rootNode in tryGetRandomNodeFromRoot');
    return null;
  }
  for (let i = 777; i--; ) {
    const nodeFromRoot = visitNodesFromRoot(nodes, rootNode, excludedQuestions);
    if (nodeFromRoot) {
      return nodeFromRoot;
    }
  }
  return null;
};

const findParentNextNode = (
  nodes: QuestionsTreeNode[],
  currentNodeId: string,
  excludedQuestions: RoomQuestion[],
): QuestionsTreeNode['question'] | null => {
  const parentNodeId = findNodeById(
    nodes,
    currentNodeId,
  )?.parentQuestionSubjectTreeId;
  if (!parentNodeId) {
    console.warn('no parentNodeId in findParentNextNode');
    return null;
  }
  const parentNode = findNodeById(nodes, parentNodeId);
  if (!parentNode) {
    console.warn('no parentNode in findParentNextNode');
    return null;
  }
  if (!parentNode.question) {
    return null;
  }
  const randomNextNode = findRandomNextNode(
    nodes,
    excludedQuestions,
    parentNode.question.id,
  );
  if (randomNextNode) {
    return randomNextNode;
  }
  return findParentNextNode(nodes, parentNode.id, excludedQuestions);
};

const getNextQuestion = (
  tree: QuestionsTree,
  excludedQuestions: RoomQuestion[],
  currentQuestionId?: string,
) => {
  const nodes = tree.tree;
  if (!currentQuestionId) {
    const randomQuestionFromRoot = tryGetRandomNodeFromRoot(
      tree,
      excludedQuestions,
    )?.question;
    return (
      randomQuestionFromRoot ||
      getRandomQuestionWithExclude(nodes, excludedQuestions)
    );
  }
  const randomNextNode = findRandomNextNode(
    nodes,
    excludedQuestions,
    currentQuestionId,
  );
  if (randomNextNode) {
    return randomNextNode;
  }
  const currentNode = findNodeByQuestionId(nodes, currentQuestionId);
  if (!currentNode) {
    console.warn('no currentNode in getNextQuestion');
    return getRandomQuestionWithExclude(nodes, excludedQuestions);
  }
  const parentNextNode = findParentNextNode(
    nodes,
    currentNode.id,
    excludedQuestions,
  );
  if (parentNextNode) {
    return parentNextNode;
  }
  const randomNodeFromRoot = tryGetRandomNodeFromRoot(tree, excludedQuestions);
  if (randomNodeFromRoot) {
    return randomNodeFromRoot.question;
  }
  return getRandomQuestionWithExclude(nodes, excludedQuestions);
};

export interface RoomQuestionPanelAiProps {
  questionWithCode: boolean;
  roomQuestionsLoading: boolean;
  roomQuestions: RoomQuestion[];
  initialQuestion?: RoomQuestion;
  children: ReactNode;
}

export const RoomQuestionPanelAi: FunctionComponent<
  RoomQuestionPanelAiProps
> = ({
  questionWithCode,
  roomQuestionsLoading,
  roomQuestions,
  initialQuestion,
  children,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const {
    room,
    roomParticipant,
    lastVoiceRecognition,
    aiAssistantScript,
    lastWsMessageParsed,
    sendWsMessage,
    setAiAssistantCurrentScript,
    setRecognitionEnabled,
  } = useContext(RoomContext);
  const { themeInUi } = useContext(ThemeContext);
  const {
    recognitionAccum,
    recognitionCommand,
    resetVoiceRecognitionAccum,
    addVoiceRecognitionAccumTranscript,
  } = useVoiceRecognitionAccum();
  const readOnly = roomParticipant?.userType !== 'Expert';
  const [roomQuestionEvaluation, setRoomQuestionEvaluation] =
    useState<RoomQuestionEvaluationValue | null>(null);
  const [questionCode, setQuestionCode] = useState<string | undefined>('');
  const [questionLanguage, setQuestionLanguage] = useState<CodeEditorLang>(
    CodeEditorLang.Javascript,
  );
  const [copilotAnswerOpen, setCopilotAnswerOpen] = useState(false);
  const startedByVoiceRef = useRef(false);
  const [questionTimerStartDate, setQuestionTimerStartDate] = useState<
    string | null
  >(null);

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
  } = useApiMethod<unknown, CompleteRoomReviewsBody>(
    roomReviewApiDeclaration.completeAi,
  );
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
      enabled: copilotAnswerOpen && !readOnly,
      answer: recognitionAccum,
      conversationId: `${room?.id}${initialQuestion?.id}${auth?.id}`,
      question: initialQuestion?.value || '',
      questionId: initialQuestion?.id || '',
      theme: room?.questionTree?.name || '',
      userId: auth?.id || '',
      taskDescription: initialQuestion?.value || '',
      code: questionCode || '',
      language: questionLanguage,
      endpoint: questionWithCode ? AiEndpoint.analyze : AiEndpoint.examinee,
    });
  const [wsLastValidAiAnswer, setWsLastValidAiAnswer] =
    useState<AnyObject | null>(null);
  const totalLastValidAiAnswer = lastValidAiAnswer || wsLastValidAiAnswer;

  const themedAiAvatar = useThemedAiAvatar();
  const allUsersWithAiExpert = new Map<string, AnalyticsUserReview>();
  allUsersWithAiExpert.set(aiExpertId, {
    comment: '',
    nickname: aiExpertNickname,
    participantType: 'Expert',
    userId: aiExpertId,
    avatar: themedAiAvatar,
  });

  const closedQuestions = useMemo(
    () =>
      roomQuestions.filter((roomQuestion) => roomQuestion.state === 'Closed'),
    [roomQuestions],
  );
  const openQuestions = roomQuestions.filter(
    (roomQuestion) => roomQuestion.state === 'Open',
  );

  const readyToReview = openQuestions.length === 0;
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
    localizationCaptions[LocalizationKey.RateMeCommands].split('|')[0],
  );

  const firstLineCaption = initialQuestion
    ? initialQuestion.value
    : localizationCaptions[LocalizationKey.WaitingInterviewStart];
  const secondLineCaption = initialQuestion
    ? rateMeDescription
    : letsStartDescription;

  useEffect(() => {
    if (readOnly) {
      return;
    }
    setRecognitionEnabled(!copilotAnswerOpen);
  }, [readOnly, copilotAnswerOpen, setRecognitionEnabled]);

  useEffect(() => {
    if (!lastVoiceRecognition || readOnly) {
      return;
    }
    addVoiceRecognitionAccumTranscript(lastVoiceRecognition);
  }, [lastVoiceRecognition, readOnly, addVoiceRecognitionAccumTranscript]);

  useEffect(() => {
    if (!lastWsMessageParsed || !readOnly) {
      return;
    }
    if (lastWsMessageParsed.Type === 'ValidAiAnswer') {
      setWsLastValidAiAnswer(lastWsMessageParsed.Value.AdditionalData);
      setCopilotAnswerOpen(true);
    }
  }, [readOnly, lastWsMessageParsed, auth?.id]);

  useEffect(() => {
    if (!initialQuestion?.id) {
      return;
    }
    setQuestionTimerStartDate(new Date().toISOString());
    resetVoiceRecognitionAccum();
  }, [initialQuestion?.id, resetVoiceRecognitionAccum]);

  useEffect(() => {
    if (!initialQuestion?.id || !readOnly) {
      return;
    }
    setWsLastValidAiAnswer(null);
    setCopilotAnswerOpen(false);
  }, [initialQuestion?.id, readOnly]);

  const handleCopilotAnswerOpen = useCallback(() => {
    setCopilotAnswerOpen(true);
  }, []);

  const handleCopilotAnswerClose = useCallback(() => {
    setCopilotAnswerOpen(false);
  }, []);

  useEffect(() => {
    if (questionWithCode) {
      return;
    }
    if (recognitionCommand !== VoiceRecognitionCommand.RateMe) {
      return;
    }
    handleCopilotAnswerOpen();
  }, [questionWithCode, recognitionCommand, handleCopilotAnswerOpen]);

  useEffect(() => {
    if (readOnly) {
      return;
    }
    if (!aiAnswerCompleted || !lastValidAiAnswer) {
      return;
    }
    setRoomQuestionEvaluation({
      mark: Math.round(lastValidAiAnswer?.score),
      review: lastValidAiAnswer?.reason,
    });
  }, [readOnly, aiAnswerCompleted, lastValidAiAnswer]);

  useEffect(() => {
    if (readOnly || !lastValidAiAnswer) {
      return;
    }
    sendWsMessage(
      JSON.stringify({
        Type: EventName.ValidAiAnswer,
        Value: JSON.stringify(lastValidAiAnswer),
      }),
    );
  }, [readOnly, lastValidAiAnswer, sendWsMessage]);

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
    if (!room.questionTree) {
      throw new Error('handleNextQuestion questionTree not found.');
    }
    const nextQuestion = getNextQuestion(
      room.questionTree,
      [...closedQuestions, ...(initialQuestion ? [initialQuestion] : [])],
      initialQuestion?.id,
    );
    if (!nextQuestion) {
      console.warn('handleNextQuestion empty nextQuestion');
      return;
    }
    handleCopilotAnswerClose();
    resetVoiceRecognitionAccum();
    sendRoomActiveQuestion({
      roomId: room.id,
      questionId: nextQuestion.id,
    });
  }, [
    room,
    closedQuestions,
    initialQuestion,
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
    fetchRoomStartReview({ roomId: room.id });
  }, [room?.id, fetchRoomStartReview]);

  const handleExecutionResultsSubmit = (
    code: string | undefined,
    language: CodeEditorLang,
  ) => {
    setQuestionCode(code);
    setQuestionLanguage(language);
    handleCopilotAnswerOpen();
  };

  const statusPanelVisible = questionWithCode ? copilotAnswerOpen : true;
  const loadingTotal =
    loadingRoomStartReview ||
    roomQuestionsLoading ||
    loadingRoomActiveQuestion ||
    aiAnswerLoading;

  return (
    <>
      <Gap sizeRem={4.375} />

      {errorRoomActiveQuestion && (
        <>
          <Gap sizeRem={1} />
          <Typography size="m" error>
            {localizationCaptions[LocalizationKey.ErrorSendingActiveQuestion]}
          </Typography>
        </>
      )}
      <div className="flex flex-1">
        <div className="flex flex-col">
          <Gap sizeRem={2.5} />
          <div
            className="bg-wrap rounded-2.5 px-8 flex-1 flex flex-col"
            style={{
              borderTopLeftRadius: 0,
              borderBottomLeftRadius: 0,
              background:
                themeInUi === Theme.Light
                  ? 'linear-gradient(270deg, rgba(67, 184, 241, 0.07) 0%, rgba(245, 246, 248, 0.07) 92.16%)'
                  : '',
            }}
          ></div>
          <Gap sizeRem={2.75} />
        </div>
        <Gap sizeRem={1.75} horizontal />
        <div
          className="relative bg-wrap flex-1 rounded-2.5 flex flex-col"
          style={{ width: '840px' }}
        >
          {!copilotAnswerOpen && questionTimerStartDate && (
            <div
              className="absolute"
              style={{ top: '1.125rem', right: '1.625rem' }}
            >
              <RoomTimerAi
                startTime={questionTimerStartDate}
                durationSec={
                  questionWithCode
                    ? questionWithCodeAnswerTimer
                    : questionAnswerTimer
                }
              />
            </div>
          )}
          {totalErrorRoomQuestionEvaluation && (
            <Typography size="m" error>
              {totalErrorRoomQuestionEvaluation}
            </Typography>
          )}
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
          {statusPanelVisible && (
            <>
              <Gap sizeRem={copilotAnswerOpen ? 1.6875 : 7.6875} />
              <div className="flex flex-col px-4.75 h-full">
                <div className="flex">
                  <div className="flex items-center justify-center">
                    <div
                      className={`z-0`}
                      style={{
                        width: '86px',
                        height: '86px',
                      }}
                    />
                    <div
                      className="absolute"
                      style={{ width: '84px', height: '84px' }}
                    >
                      <Canvas
                        shadows
                        camera={{ position: [0, 0.12, 1.5], fov: 38 }}
                      >
                        <AiAssistant
                          loading={loadingTotal}
                          currentScript={
                            readOnly
                              ? AiAssistantScriptName.Idle
                              : aiAssistantScript
                          }
                        />
                      </Canvas>
                    </div>
                  </div>
                  <Gap sizeRem={2.1875} horizontal />
                  <div className="flex flex-col text-left justify-center">
                    <Typography size="xl">{firstLineCaption}</Typography>
                  </div>
                </div>
                {!copilotAnswerOpen && (
                  <div className="text-left">
                    <Gap sizeRem={3.625} />
                    <div className="flex">
                      <Gap sizeRem={7.5} horizontal />
                      <Typography size="s" secondary>
                        {secondLineCaption}
                      </Typography>
                    </div>
                  </div>
                )}
                {copilotAnswerOpen && (
                  <>
                    <Button
                      variant="invertedActive"
                      className="absolute min-w-unset w-2.5 h-2.5 p-0 z-1"
                      style={{
                        right: '-1.25rem',
                        top: 'calc(50% - 1.25rem)',
                      }}
                      disabled={nextQuestionButtonLoading}
                      onClick={
                        readyToReview
                          ? handleStartReviewRoom
                          : handleNextQuestion
                      }
                    >
                      {nextQuestionButtonLoading ? (
                        <Loader />
                      ) : (
                        <Icon size="s" name={IconNames.ChevronForward} />
                      )}
                    </Button>
                    <Gap sizeRem={2} />
                    <div className="flex flex-1">
                      <div
                        className={`flex-1 flex flex-col text-left px-1.875 h-full rounded-1.5 ${themeInUi === Theme.Dark ? 'bg-dark-dark1' : ''}`}
                        style={{
                          background:
                            themeInUi === Theme.Light
                              ? 'linear-gradient(180.08deg, #F6F9FF 3.79%, #FFFFFF 105.65%)'
                              : '',
                        }}
                      >
                        <Gap sizeRem={1.375} />
                        <div className="flex justify-between">
                          <div className="flex flex-col">
                            <Typography size="xxxl" bold>
                              {totalLastValidAiAnswer?.score}{' '}
                              {localizationCaptions[LocalizationKey.From10]}
                            </Typography>
                            <Typography size="m" secondary>
                              {
                                localizationCaptions[
                                  LocalizationKey.AnswerEvaluation
                                ]
                              }
                            </Typography>
                          </div>
                          <div className="flex flex-col">
                            <Typography size="xxxl">
                              <Icon inheritFontSize name={IconNames.Happy} />
                            </Typography>
                            <Typography size="m" secondary>
                              {
                                localizationCaptions[
                                  LocalizationKey.EmotionalAssessment
                                ]
                              }
                            </Typography>
                          </div>
                        </div>
                        <Gap sizeRem={1.875} />
                        <Typography size="m">
                          {totalLastValidAiAnswer?.reason ||
                            totalLastValidAiAnswer?.codeReadability}
                        </Typography>
                        <Gap sizeRem={1.375} />
                      </div>
                      <Gap sizeRem={0.625} horizontal />
                      <div
                        className={`flex-1 flex flex-col text-left px-1.875 h-full rounded-1.5 ${themeInUi === Theme.Dark ? 'bg-dark-dark1' : ''}`}
                        style={{
                          background:
                            themeInUi === Theme.Light
                              ? 'linear-gradient(359.67deg, #FFFFFF 0.27%, #FDF8FF 97.33%)'
                              : '',
                        }}
                      >
                        <Gap sizeRem={2} />
                        <Typography size="m" bold>
                          {
                            localizationCaptions[
                              LocalizationKey.ExampleOfCorrectAnswer
                            ]
                          }
                        </Typography>
                        <Gap sizeRem={1.375} />
                        <Typography size="m">
                          {totalLastValidAiAnswer?.expected}
                        </Typography>
                      </div>
                    </div>
                    <Gap sizeRem={3.125} />
                  </>
                )}
              </div>
            </>
          )}
          {!statusPanelVisible && (
            <div className="flex-1">
              <RoomCodeEditor
                // TODO: Fix this hardcode ( https://github.com/sorface/interview/issues/650 )
                language={CodeEditorLang.Javascript}
                visible={!copilotAnswerOpen && questionWithCode}
                onExecutionResultsSubmit={handleExecutionResultsSubmit}
              />
            </div>
          )}

          {children}
          {!statusPanelVisible && (
            <div
              className="absolute flex items-center justify-center z-60"
              style={{
                bottom: '97px',
                right: '44px',
              }}
            >
              <div
                className={`z-0`}
                style={{
                  width: '86px',
                  height: '86px',
                }}
              />
              <div
                className="absolute"
                style={{ width: '84px', height: '84px' }}
              >
                <Canvas shadows camera={{ position: [0, 0.12, 1.5], fov: 38 }}>
                  <AiAssistant
                    loading={loadingTotal}
                    currentScript={
                      readOnly ? AiAssistantScriptName.Idle : aiAssistantScript
                    }
                  />
                </Canvas>
              </div>
            </div>
          )}
        </div>
        <Gap sizeRem={1.75} horizontal />
        <div className="flex flex-col">
          <Gap sizeRem={2.5} />
          <div
            className="bg-wrap rounded-2.5 px-8 flex-1 flex flex-col"
            style={{
              borderTopRightRadius: 0,
              borderBottomRightRadius: 0,
              background:
                themeInUi === Theme.Light
                  ? 'linear-gradient(89.03deg, #EFF0FF -2.02%, #F5F6F8 96.31%)'
                  : '',
            }}
          ></div>
          <Gap sizeRem={2.75} />
        </div>
      </div>
    </>
  );
};
