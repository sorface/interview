import { useEffect, useState } from 'react';
import { tryCompleteJson } from '../utils/tryCompleteJson';
import { AnyObject } from '../../../types/anyObject';
import { VITE_AI_API } from '../../../config';

interface UseAiAnswerSourceParams {
  enabled: boolean;
  theme: string;
  question: string;
  answer: string;
  conversationId: string;
  questionId: string;
  userId: string;
}

export const useAiAnswerSource = ({
  enabled,
  theme,
  question,
  answer,
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
      return;
    }
    const abortController = new AbortController();
    setlastValidAiAnswer(null);
    setCompleted(false);
    setLoading(true);

    const fetchAiEvaluate = async () => {
      const response = await fetch(`${VITE_AI_API}/ai-assistant/examinee`, {
        headers: {
          'content-type': 'application/json',
        },
        body: JSON.stringify({
          theme,
          question,
          answer,
          conversationId,
          questionId,
          userId,
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
  }, [enabled, theme, question, answer, conversationId, questionId, userId]);

  return {
    lastValidAiAnswer,
    aiAnswerCompleted: completed,
    aiAnswerLoading: loading,
  };
};
