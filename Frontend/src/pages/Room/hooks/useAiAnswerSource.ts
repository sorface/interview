import { useEffect, useState } from 'react';
import { tryCompleteJson } from '../utils/tryCompleteJson';
import { AnyObject } from '../../../types/anyObject';
import { VITE_AI_API } from '../../../config';

export const enum AiEndpoint {
  examinee = 'examinee',
  analyze = 'analyze',
}

interface UseAiAnswerSourceParams {
  enabled: boolean;
  endpoint: AiEndpoint;
  theme: string;
  question: string;
  answer: string;
  taskDescription: string;
  code: string;
  language: string;
  conversationId: string;
  questionId: string;
  userId: string;
}

export const useAiAnswerSource = ({
  enabled,
  endpoint,
  theme,
  question,
  answer,
  taskDescription,
  code,
  language,
  conversationId,
  questionId,
  userId,
}: UseAiAnswerSourceParams) => {
  const [lastValidAiAnswer, setlastValidAiAnswer] = useState<null | AnyObject>(
    null,
  );
  const [completed, setCompleted] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!enabled) {
      setlastValidAiAnswer(null);
      setCompleted(false);
      setLoading(false);
      return;
    }
    const abortController = new AbortController();
    setlastValidAiAnswer(null);
    setCompleted(false);
    setLoading(true);

    const fetchAiEvaluate = async () => {
      const response = await fetch(`${VITE_AI_API}/ai-assistant/${endpoint}`, {
        credentials: 'include',
        headers: {
          accept: 'text/event-stream',
          'content-type': 'application/json',
        },
        body: JSON.stringify({
          conversationId,
          questionId,
          userId,
          ...(endpoint === AiEndpoint.examinee
            ? {
                theme,
                question,
                answer,
              }
            : {
                taskDescription,
                code,
                language,
              }),
        }),
        method: 'POST',
        signal: abortController.signal,
      });
      const body = response.body;
      if (!body) {
        return;
      }
      const reader = body.pipeThrough(new TextDecoderStream()).getReader();

      let accum = '';
      while (true) {
        const { done, value } = await reader.read();
        if (done) {
          break;
        }
        const normalizedValue = value
          ?.replaceAll('data:', '')
          .replaceAll('\n', '');
        accum += normalizedValue;
        const parsedAiAnswerAccum = tryCompleteJson(accum);
        if (parsedAiAnswerAccum) {
          setlastValidAiAnswer(parsedAiAnswerAccum);
        }
      }
      setCompleted(true);
      setLoading(false);
    };
    fetchAiEvaluate();

    return () => {
      abortController.abort();
    };
  }, [
    enabled,
    theme,
    question,
    answer,
    conversationId,
    questionId,
    userId,
    endpoint,
    taskDescription,
    code,
    language,
  ]);

  return {
    lastValidAiAnswer,
    aiAnswerCompleted: completed,
    aiAnswerLoading: loading,
  };
};
