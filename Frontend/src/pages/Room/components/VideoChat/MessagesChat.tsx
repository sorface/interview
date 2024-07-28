import { FunctionComponent, useRef, KeyboardEvent, useEffect, useContext } from 'react';
import { Transcript } from '../../../../types/transcript';
import { stringToColor } from './utils/stringToColor';
import { ThemeContext } from '../../../../context/ThemeContext';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';
import { Gap } from '../../../../components/Gap/Gap';

import './MessagesChat.css';

interface MessagesChatProps {
  textMessages: Transcript[];
  onMessageSubmit: (message: string) => void;
}

export const MessagesChat: FunctionComponent<MessagesChatProps> = ({
  textMessages,
  onMessageSubmit,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const { themeInUi } = useContext(ThemeContext);
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
      <Gap sizeRem={0.75} />
      <div className='videochat-transcripts px-0.75' ref={videochatTranscriptsRef}>
        {textMessages.map(transcript => (
          <div
            key={transcript.frontendId}
          >
            <span
              style={{ color: stringToColor(transcript.userNickname, themeInUi) }}
            >
              {transcript.userNickname}
            </span>
            {': '}
            {transcript.value}
          </div>
        ))}
      </div>
      <div className='flex justify-between px-0.75'>
        <input
          type='text'
          placeholder={localizationCaptions[LocalizationKey.ChatMessagePlaceholder]}
          ref={messageInputRef}
          className='w-7.5'
          onKeyDown={handleInputKeyDown}
        />
        <Button
          className='min-w-fit !transition'
          onClick={handleChatMessageSubmit}>
          {localizationCaptions[LocalizationKey.SendToChat]}
        </Button>
      </div>
      <Gap sizeRem={0.75} />
    </div>
  );
};
