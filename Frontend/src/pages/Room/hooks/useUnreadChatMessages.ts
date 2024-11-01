import { useEffect, useRef, useState } from 'react';
import { ParsedWsMessage } from '../utils/parseWsMessage';

interface UseUnreadChatMessagesParams {
  lastWsMessageParsed: ParsedWsMessage | null;
  messagesChatEnabled: boolean;
  maxCount: number;
}

export const useUnreadChatMessages = ({
  lastWsMessageParsed,
  messagesChatEnabled,
  maxCount,
}: UseUnreadChatMessagesParams) => {
  const unreadChatMessagesRef = useRef(0);
  const [unreadChatMessages, setUnreadChatMessages] = useState(0);
  const [lastReadMessageId, setLastReadMessageId] = useState('');

  useEffect(() => {
    try {
      if (!lastWsMessageParsed) {
        return;
      }
      if (lastWsMessageParsed.Type !== 'ChatMessage') {
        return;
      }
      if (messagesChatEnabled) {
        setLastReadMessageId(lastWsMessageParsed.Id);
        return;
      }
      if (lastWsMessageParsed.Id === lastReadMessageId) {
        return;
      }
      setLastReadMessageId(lastWsMessageParsed.Id);
      unreadChatMessagesRef.current++;
      setUnreadChatMessages(unreadChatMessagesRef.current);
    } catch { }
  }, [lastWsMessageParsed, messagesChatEnabled, unreadChatMessages, lastReadMessageId]);

  useEffect(() => {
    if (!messagesChatEnabled) {
      return;
    }
    unreadChatMessagesRef.current = 0;
    setUnreadChatMessages(unreadChatMessagesRef.current);
  }, [messagesChatEnabled]);

  return {
    unreadChatMessages: unreadChatMessages > maxCount ? `${maxCount}+` : unreadChatMessages,
  };
};
