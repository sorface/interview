import { useEffect, useRef, useState } from 'react';
import { User } from '../../../types/user';

interface UseReactionsStatusParams {
  lastMessage: MessageEvent<any> | null
}

interface ReactionTimeout {
  reactionType: string;
  remainingTime: number;
};

type UserReactionsTimeout = Map<User['id'], ReactionTimeout | null>;

type ActiveReactions = Record<User['id'], string | null>;

export const handledEventTypes = ['Like', 'Dislike'];

const updateIntervalMs = 500;
const addReactionTimeoutMs = 3000.0;

const addReactionTimeout = (
  userReactionsTimeout: UserReactionsTimeout,
  userId: User['id'],
  reactionType: string,
) => {
  userReactionsTimeout.set(userId, {
    reactionType: reactionType,
    remainingTime: addReactionTimeoutMs,
  });
  return;
};

const updateTimeouts = (
  userReactionsTimeout: UserReactionsTimeout,
  delta: number,
) => {
  let updated = false;
  userReactionsTimeout.forEach((reactionTimeout, userId) => {
    if (!reactionTimeout) {
      return;
    }
    reactionTimeout.remainingTime -= delta;
    if (reactionTimeout.remainingTime <= 0) {
      userReactionsTimeout.set(userId, null);
      updated = true;
    }
  });
  return updated;
};

const timeoutToActiveReactions = (
  userReactionsTimeout: UserReactionsTimeout,
): ActiveReactions => {
  const activeReactions: ActiveReactions = {};
  userReactionsTimeout.forEach((reactionTimeout, userId) => {
    if (!reactionTimeout) {
      return;
    }
    const reactionType = reactionTimeout.reactionType;
    activeReactions[userId] = reactionType;
  });
  return activeReactions;
};

export const useReactionsStatus = ({
  lastMessage,
}: UseReactionsStatusParams) => {
  const userReactionsTimeoutRef = useRef<UserReactionsTimeout>(new Map());
  const [activeReactions, setActiveReactions] = useState<ActiveReactions>({});

  useEffect(() => {
    let prevTime = performance.now();
    const updateReactionsTimeout = () => {
      const time = performance.now();
      const delta = time - prevTime;
      const updated = updateTimeouts(userReactionsTimeoutRef.current, delta);
      if (updated) {
        setActiveReactions(timeoutToActiveReactions(userReactionsTimeoutRef.current));
      }
      prevTime = time;
    };
    const intervalId = setInterval(updateReactionsTimeout, updateIntervalMs);

    return () => {
      clearInterval(intervalId);
    };
  }, []);

  useEffect(() => {
    if (!lastMessage) {
      return;
    }
    const parsedData = JSON.parse(lastMessage?.data);
    if (!handledEventTypes.includes(parsedData?.Type)) {
      return;
    }
    const userId = parsedData?.Value.UserId;
    if (!userId) {
      return;
    }
    addReactionTimeout(
      userReactionsTimeoutRef.current,
      userId,
      parsedData?.Type,
    );
    setActiveReactions(timeoutToActiveReactions(userReactionsTimeoutRef.current));

  }, [lastMessage]);

  return {
    activeReactions,
  };
};
