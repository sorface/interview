import { FunctionComponent, useRef, KeyboardEvent, useEffect, useContext } from 'react';
import { Transcript } from '../../../../types/transcript';
import { Theme } from '../../../../context/ThemeContext';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';
import { Gap } from '../../../../components/Gap/Gap';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { IconNames } from '../../../../constants';
import { Icon } from '../Icon/Icon';
import { ChatMessage } from './ChatMessage';
import { User } from '../../../../types/user';
import { AuthContext } from '../../../../context/AuthContext';

import './MessagesChat.css';

interface MessagesChatProps {
  textMessages: Transcript[];
  allUsers: Map<User['id'], Pick<User, 'nickname' | 'avatar'>>;
  onMessageSubmit: (message: string) => void;
}

export const MessagesChat: FunctionComponent<MessagesChatProps> = ({
  textMessages,
  allUsers,
  onMessageSubmit,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const chatButtonVariant = useThemeClassName({
    [Theme.Dark]: 'inverted' as const,
    [Theme.Light]: 'invertedActive' as const,
  });
  const messageInputRef = useRef<HTMLInputElement>(null);
  const videochatTranscriptsRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const chatEl = videochatTranscriptsRef.current;
    if (!chatEl) {
      return;
    }
    const scrollHeight = chatEl.scrollHeight;
    const height = chatEl.clientHeight;
    const maxScrollTop = scrollHeight - height;
    chatEl.scrollTop = maxScrollTop > 0 ? maxScrollTop : 0;
  }, [textMessages]);

  const handleChatMessageSubmit = () => {
    if (!messageInputRef.current) {
      console.error('message input ref not found');
      return;
    }
    const messageValue = messageInputRef.current.value;
    if (!messageValue) {
      return;
    }
    onMessageSubmit(messageValue.trim());
    messageInputRef.current.value = '';
  };

  const handleInputKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      handleChatMessageSubmit();
    }
  };

  return (
    <div className='messages-chat'>
      <div className='videochat-transcripts px-0.75' ref={videochatTranscriptsRef}>
        <Gap sizeRem={1.5} />
        {textMessages.map((transcript, index, allTranscripts) => (
          <ChatMessage
            key={transcript.id}
            createdAt={transcript.createdAt}
            message={transcript.value}
            nickname={transcript.userNickname}
            avatar={allUsers.get(transcript.userId)?.avatar}
            removePaggingTop={index === 0}
            stackWithPrevious={allTranscripts[index - 1]?.userId === transcript.userId}
            fromCurrentUser={!!auth && auth.id === transcript.userId}
          />
        ))}
      </div>
      <Gap sizeRem={0.5} />
      <div className='flex justify-between px-0.75'>
        <input
          type='text'
          placeholder={localizationCaptions[LocalizationKey.ChatMessagePlaceholder]}
          ref={messageInputRef}
          className='flex-1'
          maxLength={1000}
          onKeyDown={handleInputKeyDown}
        />
        <Gap sizeRem={0.25} horizontal />
        <Button
          variant={chatButtonVariant}
          className='min-w-fit !transition rounded-full p-0 w-2.125 h-2.125 min-h-unset'
          onClick={handleChatMessageSubmit}
        >
          <Icon size='s' name={IconNames.PaperPlane} />
        </Button>
      </div>
      <Gap sizeRem={0.75} />
    </div>
  );
};
