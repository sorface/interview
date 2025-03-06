import React, { FunctionComponent, useRef, useEffect, ReactNode } from 'react';
import { ChatMessageAi } from '../ChatMessageAi/ChatMessageAi';

export type MessagesChatAiMessage = {
  id: string;
  userId: string;
  userNickname: string;
  value: string;
  fromAi: boolean;
  children?: ReactNode;
};

interface MessagesChatAiProps {
  messages: MessagesChatAiMessage[];
}

export const MessagesChatAi: FunctionComponent<MessagesChatAiProps> = ({
  messages,
}) => {
  const videochatTranscriptsRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const chatEl = videochatTranscriptsRef.current;
    if (!chatEl) {
      return;
    }
    const scrollHeight = chatEl.scrollHeight;
    const height = chatEl.clientHeight;
    const maxScrollTop = scrollHeight - height;
    if (maxScrollTop <= 0) {
      return;
    }
    chatEl.scrollBy({
      top: maxScrollTop,
      behavior: 'smooth',
    });
  }, [messages]);

  return (
    <div className="flex-1 overflow-auto px-0.75" ref={videochatTranscriptsRef}>
      {messages.map((transcript, index, allTranscripts) => (
        <ChatMessageAi
          key={transcript.id}
          message={transcript.value}
          nickname={transcript.userNickname}
          fromAi={transcript.fromAi}
          removePaggingTop={index === 0}
          stackWithPrevious={
            allTranscripts[index - 1]?.userId === transcript.userId
          }
        >
          {transcript.children}
        </ChatMessageAi>
      ))}
    </div>
  );
};
