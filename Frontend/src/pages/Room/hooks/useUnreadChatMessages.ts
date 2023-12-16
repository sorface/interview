import { useEffect, useRef, useState } from 'react';

interface UseUnreadChatMessagesParams {
  lastMessage: MessageEvent<any> | null;
  messagesChatEnabled: boolean;
}

export const useUnreadChatMessages = ({
  lastMessage,
  messagesChatEnabled,
}: UseUnreadChatMessagesParams) => {
  const unreadChatMessagesRef = useRef(0);
  const [unreadChatMessages, setUnreadChatMessages] = useState(0);
  const [lastReadMessageId, setLastReadMessageId] = useState('');

  useEffect(() => {
    try {
      const parsedData = JSON.parse(lastMessage?.data);
      if (parsedData?.Type !== 'ChatMessage') {
        return;
      }
      if (messagesChatEnabled) {
        setLastReadMessageId(parsedData?.Id);
        return;
      }
      if (parsedData?.Id === lastReadMessageId) {
        return;
      }
      setLastReadMessageId(parsedData?.Id);
      unreadChatMessagesRef.current++;
      setUnreadChatMessages(unreadChatMessagesRef.current);
    } catch { }
  }, [lastMessage, messagesChatEnabled, unreadChatMessages, lastReadMessageId]);

  useEffect(() => {
    if (!messagesChatEnabled) {
      return;
    }
    unreadChatMessagesRef.current = 0;
    setUnreadChatMessages(unreadChatMessagesRef.current);
  }, [messagesChatEnabled]);

  return {
    unreadChatMessages,
  };
};
