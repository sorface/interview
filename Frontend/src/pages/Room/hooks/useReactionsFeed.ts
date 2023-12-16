import { useEffect, useRef, useState } from 'react';
import { handledEventTypes } from './useReactionsStatus';

interface UseReactionsFeedParams {
  lastMessage: MessageEvent<any> | null;
}

export type ReactionsFeed = Record<string, number>;

export const useReactionsFeed = ({
  lastMessage,
}: UseReactionsFeedParams) => {
  const [reactionsFeed, setReactionsFeed] = useState<ReactionsFeed>({});
  const reactionsFeedRef = useRef<ReactionsFeed>({});

  useEffect(() => {
    if (!lastMessage) {
      return;
    }
    const parsedData = JSON.parse(lastMessage?.data);
    if (!handledEventTypes.includes(parsedData?.Type)) {
      return;
    }
    reactionsFeedRef.current[parsedData?.Type] = parsedData?.Id || Math.random();
    setReactionsFeed({ ...reactionsFeedRef.current });
  }, [lastMessage]);

  return {
    reactionsFeed,
  };
};
