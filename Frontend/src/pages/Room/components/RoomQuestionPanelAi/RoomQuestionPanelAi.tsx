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
import { Button, ButtonVariant } from '../../../../components/Button/Button';
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
import { CodeEditor } from '../../../../components/CodeEditor/CodeEditor';
import { Wave } from '../Wave/Wave';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Textarea } from '../../../../components/Textarea/Textarea';
import { DeviceContext } from '../../../../context/DeviceContext';

const notFoundCode = 404;
const aiAssistantGoodRate = 6;

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

const scaleScore = (score: number) => {
  if (score >= 8) {
    return 10;
  }
  if (score === 2) {
    return 1;
  }
  return score;
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
  const device = useContext(DeviceContext);
  const localizationCaptions = useLocalizationCaptions();
  const {
    room,
    roomParticipant,
    lastVoiceRecognition,
    aiAssistantScript,
    lastWsMessageParsed,
    recognitionEnabled,
    recognitionNotSupported,
    recognitionNotAllowed,
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
  const [textAnswerOpen, setTextAnswerOpen] = useState(false);
  const [textAnswer, setTextAnswer] = useState('');

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

  const themedStartAnswerButton = useThemeClassName({
    [Theme.Dark]: 'active2' as ButtonVariant,
    [Theme.Light]: 'active' as ButtonVariant,
  });
  const themedAnswerAgainButton = useThemeClassName({
    [Theme.Dark]: 'inverted' as ButtonVariant,
    [Theme.Light]: 'active2' as ButtonVariant,
  });

  const { aiAnswerCompleted, aiAnswerLoading, lastValidAiAnswer } =
    useAiAnswerSource({
      enabled: copilotAnswerOpen && !readOnly,
      answer: textAnswerOpen ? textAnswer : recognitionAccum,
      conversationId: `${room?.id}${initialQuestion?.id}${auth?.id}`,
      question: initialQuestion?.value || '',
      questionId: initialQuestion?.id || '',
      theme:
        room?.questionTree?.themeAiDescription ||
        room?.questionTree?.name ||
        '',
      userId: auth?.id || '',
      taskDescription: initialQuestion?.value || '',
      code: questionCode || '',
      language: questionLanguage,
      endpoint: questionWithCode ? AiEndpoint.analyze : AiEndpoint.examinee,
    });
  const [wsLastValidAiAnswer, setWsLastValidAiAnswer] =
    useState<AnyObject | null>(null);
  const totalLastValidAiAnswer = lastValidAiAnswer || wsLastValidAiAnswer;
  const scaledScore =
    totalLastValidAiAnswer?.score &&
    scaleScore(Number(totalLastValidAiAnswer?.score));

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
    if (!room?.questionTree) {
      return;
    }
    localStorage.setItem(
      room.questionTree.id,
      JSON.stringify({
        closed: closedQuestions.length,
        all: roomQuestions.length,
      }),
    );
  }, [roomQuestions, closedQuestions, room?.questionTree]);

  useEffect(() => {
    if (readOnly) {
      return;
    }
    if (copilotAnswerOpen || initialQuestion) {
      setRecognitionEnabled(false);
    }
  }, [readOnly, copilotAnswerOpen, initialQuestion, setRecognitionEnabled]);

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
      mark: Math.round(scaledScore),
      review: lastValidAiAnswer?.reason,
    });
  }, [readOnly, aiAnswerCompleted, scaledScore, lastValidAiAnswer]);

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
    if (scaledScore >= aiAssistantGoodRate) {
      setAiAssistantCurrentScript(AiAssistantScriptName.GoodAnswer);
    } else {
      setAiAssistantCurrentScript(AiAssistantScriptName.NeedTrain);
    }
  }, [aiAnswerCompleted, scaledScore, setAiAssistantCurrentScript]);

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
    if (!roomQuestionEvaluation || roomQuestionEvaluation.mark === null) {
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
    setTextAnswerOpen(false);
    setTextAnswer('');
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
    localStorage.setItem(
      room?.questionTree?.id || '',
      JSON.stringify({
        closed: roomQuestions.length,
        all: roomQuestions.length,
      }),
    );
    fetchRoomStartReview({ roomId: room.id });
  }, [
    room?.id,
    room?.questionTree?.id,
    roomQuestions.length,
    fetchRoomStartReview,
  ]);

  const handleNextQuestionOrReview = useCallback(() => {
    if (readyToReview) {
      handleStartReviewRoom();
      return;
    }
    handleNextQuestion();
  }, [readyToReview, handleStartReviewRoom, handleNextQuestion]);

  const handleExecutionResultsSubmit = (
    code: string | undefined,
    language: CodeEditorLang,
  ) => {
    setQuestionCode(code);
    setQuestionLanguage(language);
    handleCopilotAnswerOpen();
  };

  const handleStartAnswer = () => {
    setRecognitionEnabled(true);
    setTextAnswerOpen(false);
  };

  const handleTextAnswerOpen = () => {
    setRecognitionEnabled(false);
    setTextAnswerOpen(true);
  };

  const handleTextAnswerChange = (
    e: React.ChangeEvent<HTMLTextAreaElement>,
  ) => {
    setTextAnswer(e.target.value);
  };

  const handleSendTextAnswer = () => {
    handleCopilotAnswerOpen();
  };

  const handleAnswerAgain = () => {
    setAiAssistantCurrentScript(AiAssistantScriptName.PleaseAnswer);
    handleCopilotAnswerClose();
    resetVoiceRecognitionAccum();
  };

  const statusPanelVisible = questionWithCode ? copilotAnswerOpen : true;
  const loadingTotal =
    loadingRoomStartReview ||
    roomQuestionsLoading ||
    loadingRoomActiveQuestion ||
    aiAnswerLoading;

  return (
    <>
      {errorRoomActiveQuestion && (
        <>
          <Gap sizeRem={1} />
          <Typography size="m" error>
            {localizationCaptions[LocalizationKey.ErrorSendingActiveQuestion]}
          </Typography>
        </>
      )}
      <div className="flex flex-1">
        {device === 'Desktop' && (
          <div className="flex flex-col">
            <Gap sizeRem={2.5} />
            <div
              className="bg-wrap rounded-[2.5rem] flex-1 flex flex-col"
              style={{
                borderTopLeftRadius: 0,
                borderBottomLeftRadius: 0,
                padding: 0,
                width: '138px',
                background:
                  themeInUi === Theme.Light
                    ? 'linear-gradient(270deg, rgba(67, 184, 241, 0.07) 0%, rgba(245, 246, 248, 0.07) 92.16%)'
                    : '',
              }}
            ></div>
            <Gap sizeRem={2.75} />
          </div>
        )}
        {device === 'Desktop' && <Gap sizeRem={1.75} horizontal />}
        <div
          className={`relative bg-wrap flex-1 rounded-[2.5rem] flex flex-col ${device === 'Mobile' && copilotAnswerOpen ? 'overflow-x-auto h-[calc(100svh-58px)]' : ''}`}
          style={{ width: device === 'Desktop' ? '840px' : '100%' }}
        >
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
              <div
                className={`flex flex-col h-full ${device === 'Desktop' ? 'px-[4.75rem]' : 'px-[1rem]'}`}
              >
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
                    <Gap sizeRem={device === 'Desktop' ? 3.625 : 1.5} />
                    <div className="flex">
                      {device === 'Desktop' && <Gap sizeRem={7.5} horizontal />}
                      <div className="flex flex-col w-full">
                        {initialQuestion && (
                          <div className="flex">
                            <Button
                              variant={themedStartAnswerButton}
                              className="w-fit"
                              disabled={
                                recognitionEnabled ||
                                recognitionNotSupported ||
                                recognitionNotAllowed
                              }
                              onClick={handleStartAnswer}
                            >
                              <Icon name={IconNames.MicOn} />
                              <Gap sizeRem={0.25} horizontal />
                              {
                                localizationCaptions[
                                  LocalizationKey.StartAnswer
                                ]
                              }
                            </Button>
                            <Gap sizeRem={0.5} horizontal />
                            <Button
                              variant="inverted"
                              className="w-fit"
                              disabled={textAnswerOpen}
                              onClick={handleTextAnswerOpen}
                            >
                              <Icon name={IconNames.FileTray} />
                              <Gap sizeRem={0.25} horizontal />
                              {localizationCaptions[LocalizationKey.TextAnswer]}
                            </Button>
                            <Gap sizeRem={0.5} horizontal />
                            <Button
                              variant="inverted"
                              className="w-fit"
                              onClick={handleNextQuestionOrReview}
                            >
                              <Icon name={IconNames.ChevronForward} />
                              <Gap sizeRem={0.25} horizontal />
                              {
                                localizationCaptions[
                                  LocalizationKey.SkipQuestion
                                ]
                              }
                            </Button>
                          </div>
                        )}
                        {recognitionNotSupported &&
                          initialQuestion &&
                          !textAnswerOpen && (
                            <>
                              <Gap sizeRem={0.5} />
                              <Typography size="s" secondary>
                                {
                                  localizationCaptions[
                                    LocalizationKey
                                      .ErrorRecognitionNotSupportedTitle
                                  ]
                                }
                              </Typography>
                              <Gap sizeRem={0.15} />
                              <Typography size="s" secondary>
                                {
                                  localizationCaptions[
                                    LocalizationKey
                                      .ErrorRecognitionNotSupportedDescription
                                  ]
                                }
                              </Typography>
                            </>
                          )}
                        {recognitionNotAllowed &&
                          initialQuestion &&
                          !textAnswerOpen && (
                            <>
                              <Gap sizeRem={0.5} />
                              <Typography size="s" secondary>
                                {
                                  localizationCaptions[
                                    LocalizationKey.AllowAccessToMicrophone
                                  ]
                                }
                              </Typography>
                              <Gap sizeRem={0.15} />
                              <Typography size="s" secondary>
                                {
                                  localizationCaptions[
                                    LocalizationKey
                                      .AllowAccessToMicrophoneDescription
                                  ]
                                }
                              </Typography>
                            </>
                          )}
                        {recognitionEnabled && !recognitionNotAllowed && (
                          <div className="flex flex-col">
                            <Wave />
                            <Typography size="s" secondary>
                              {secondLineCaption}
                            </Typography>
                          </div>
                        )}
                        {textAnswerOpen && (
                          <div className="flex flex-col">
                            <Gap sizeRem={0.75} />
                            <Textarea
                              value={textAnswer}
                              maxLength={1000}
                              showMaxLength
                              className="w-full h-[8rem]"
                              onChange={handleTextAnswerChange}
                            />
                            <Gap sizeRem={0.5} />
                            <Button
                              variant="active2"
                              className="w-fit"
                              onClick={handleSendTextAnswer}
                            >
                              <Icon name={IconNames.Share} />
                              <Gap sizeRem={0.25} horizontal />
                              {localizationCaptions[LocalizationKey.SendAnswer]}
                            </Button>
                          </div>
                        )}
                        {!initialQuestion && (
                          <Button
                            variant={themedStartAnswerButton}
                            className="w-fit"
                            onClick={handleNextQuestionOrReview}
                          >
                            <Icon name={IconNames.PlayOutline} />
                            <Gap sizeRem={0.25} horizontal />
                            {
                              localizationCaptions[
                                LocalizationKey.StartInterview
                              ]
                            }
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>
                )}
                {copilotAnswerOpen && (
                  <>
                    {device === 'Mobile' && <Gap sizeRem={1} />}
                    <Button
                      variant={
                        device === 'Desktop' ? 'invertedActive' : 'active'
                      }
                      className={`${device === 'Desktop' ? ' absolute' : 'w-full'} min-w-[0rem] w-[2.5rem] h-[2.5rem] !p-[0rem] z-1`}
                      style={{
                        right: '-1.25rem',
                        top: 'calc(50% - 1.25rem)',
                      }}
                      disabled={nextQuestionButtonLoading}
                      onClick={handleNextQuestionOrReview}
                    >
                      {nextQuestionButtonLoading ? (
                        <Loader />
                      ) : (
                        <>
                          {device === 'Mobile' && (
                            <>
                              <Gap sizeRem={0.25} horizontal />
                              <Typography size="m">
                                {
                                  localizationCaptions[
                                    LocalizationKey.NextRoomQuestion
                                  ]
                                }
                              </Typography>
                            </>
                          )}
                          <Icon size="s" name={IconNames.ChevronForward} />
                        </>
                      )}
                    </Button>
                    <Gap sizeRem={device === 'Desktop' ? 2 : 1} />
                    <div
                      className={`flex flex-1 ${device === 'Mobile' ? 'flex-col' : ''}`}
                    >
                      <div
                        className={`flex-1 flex flex-col text-left px-[1.875rem] h-full rounded-[1.5rem] ${questionWithCode ? 'max-w-[400px]' : ''} ${themeInUi === Theme.Dark ? 'bg-dark-dark1' : ''}`}
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
                              {scaledScore}{' '}
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
                        </div>
                        <Gap sizeRem={1.875} />
                        <Typography size="m">
                          {totalLastValidAiAnswer?.reason ||
                            totalLastValidAiAnswer?.codeReadability}
                        </Typography>
                        <Gap sizeRem={1.375} />
                        <Button
                          variant={themedAnswerAgainButton}
                          className="!w-fit"
                          disabled={!aiAnswerCompleted}
                          onClick={handleAnswerAgain}
                        >
                          {aiAnswerCompleted ? (
                            <>
                              <Icon name={IconNames.Sync} />
                              <Gap sizeRem={0.25} horizontal />
                              {
                                localizationCaptions[
                                  LocalizationKey.AnswerAgain
                                ]
                              }
                            </>
                          ) : (
                            <Loader />
                          )}
                        </Button>
                      </div>
                      <Gap sizeRem={0.625} horizontal={device === 'Desktop'} />
                      <div
                        className={`flex-1 flex flex-col text-left px-[1.875rem] h-full rounded-[1.5rem] ${themeInUi === Theme.Dark ? 'bg-dark-dark1' : ''}`}
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
                        {totalLastValidAiAnswer?.expected && (
                          <Typography size="m">
                            {totalLastValidAiAnswer?.expected}
                          </Typography>
                        )}
                        {totalLastValidAiAnswer?.referenceCode && (
                          <CodeEditor
                            language={CodeEditorLang.Javascript}
                            languages={[CodeEditorLang.Javascript]}
                            readOnly
                            value={totalLastValidAiAnswer?.referenceCode}
                            className={device === 'Mobile' ? 'h-[30rem]' : ''}
                          />
                        )}
                        <Gap sizeRem={2} />
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
                onSkip={handleNextQuestionOrReview}
              />
            </div>
          )}

          {children}
          {!statusPanelVisible && (
            <div
              className="absolute flex items-center justify-center z-20"
              style={{
                bottom: '12px',
                ...(device === 'Desktop' && { right: '12px' }),
                ...(device === 'Mobile' && { left: '12px' }),
              }}
            >
              <div
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
        {device === 'Desktop' && <Gap sizeRem={1.75} horizontal />}
        {device === 'Desktop' && (
          <div className="flex flex-col">
            <Gap sizeRem={2.5} />
            <div
              className="bg-wrap rounded-[2.5rem] flex-1 flex flex-col"
              style={{
                borderTopRightRadius: 0,
                borderBottomRightRadius: 0,
                padding: 0,
                width: '138px',
                background:
                  themeInUi === Theme.Light
                    ? 'linear-gradient(89.03deg, #EFF0FF -2.02%, #F5F6F8 96.31%)'
                    : '',
              }}
            ></div>
            <Gap sizeRem={2.75} />
          </div>
        )}
      </div>
    </>
  );
};
