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
import {
  OtherComment,
  ReviewUserOpinion,
} from '../../../RoomAnaytics/components/ReviewUserOpinion/ReviewUserOpinion';
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
import {
  MessagesChatAi,
  MessagesChatAiMessage,
} from '../MessagesChatAi/MessagesChatAi';

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
  for (let i = 777; i--;) {
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
  for (let i = 777; i--;) {
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

const getCustomCommets = (
  lastValidAiAnswer: AnyObject | null,
): OtherComment[] => {
  if (!lastValidAiAnswer) {
    return [];
  }

  const localizations = {
    recommendation: LocalizationKey.Recommendation,
    expected: LocalizationKey.ExampleOfCorrectAnswer,
    performance: LocalizationKey.CodePerformance,
    bestPractice: LocalizationKey.BestPractice,
    vulnerabilities: LocalizationKey.Vulnerabilities,
    comments: LocalizationKey.CodeComments,
    refactoringProposal: LocalizationKey.RefactoringProposal,
    referenceCode: LocalizationKey.ReferenceCode,
  };

  const comments: OtherComment[] = [];
  Object.entries(localizations).forEach(([key, value]) => {
    const comment = lastValidAiAnswer[key];
    if (!comment) {
      return;
    }
    comments.push({
      title: value,
      value: comment,
    });
  });
  return comments;
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
    // const [chatMessages, setChatMessages] = useState<MessagesChatAiMessage[]>([]);

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
              {
                localizationCaptions[
                LocalizationKey.ErrorSendingActiveQuestion
                ]
              }
            </Typography>
          </>
        )}
        <div className='flex flex-1'>
          <div
            className='bg-wrap flex-0 rounded-2.5 px-8 flex flex-col'
            style={{
              borderTopLeftRadius: 0,
              borderBottomLeftRadius: 0,
              background: 'linear-gradient(270deg, rgba(67, 184, 241, 0.07) 0%, rgba(245, 246, 248, 0.07) 92.16%)',
            }}
          ></div>
          <Gap sizeRem={1.75} horizontal />
          <div className='relative bg-wrap flex-1 rounded-2.5 flex flex-col' style={{ width: '840px' }}>
            {statusPanelVisible && (
              <>
                <Gap sizeRem={4.75} />
                <div className='flex px-8'>
                  <div className="flex items-center justify-center">
                    <div
                      className={`z-0`}
                      style={{
                        width: '72px',
                        height: '72px',
                      }}
                    />
                    <div className="absolute" style={{ width: '72px', height: '72px' }}>
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
                  <Gap sizeRem={1.375} horizontal />
                  <div className='flex flex-col text-left justify-center'>
                    <Typography size='xl'>{firstLineCaption}</Typography>
                    <Typography size='m'>{secondLineCaption}</Typography>
                  </div>
                </div>
              </>
            )}
            {!statusPanelVisible && (
              <div className='flex-1'>
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
                    width: '72px',
                    height: '72px',
                  }}
                />
                <div className="absolute" style={{ width: '72px', height: '72px' }}>
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
          <div
            className='bg-wrap flex-0 rounded-2.5 px-8 flex flex-col'
            style={{
              borderTopRightRadius: 0,
              borderBottomRightRadius: 0,
              background: 'linear-gradient(89.03deg, #EFF0FF -2.02%, #F5F6F8 96.31%)',
            }}
          ></div>
        </div>
        <Gap sizeRem={1} />
      </>
    );
  };
