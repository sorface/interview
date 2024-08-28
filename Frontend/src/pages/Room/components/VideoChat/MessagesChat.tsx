import { FunctionComponent, useRef, KeyboardEvent, useEffect, Fragment } from 'react';
import { Transcript } from '../../../../types/transcript';
import { Theme } from '../../../../context/ThemeContext';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';
import { Gap } from '../../../../components/Gap/Gap';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Typography } from '../../../../components/Typography/Typography';
import { padTime } from '../../../../utils/padTime';
import { IconNames } from '../../../../constants';
import { Icon } from '../Icon/Icon';

import './MessagesChat.css';

interface MessagesChatProps {
  textMessages: Transcript[];
  onMessageSubmit: (message: string) => void;
}

const formatTime = (value: Date) => {
  const hours = padTime(value.getHours());
  const minutes = padTime(value.getMinutes());
  return `${hours}:${minutes}`;
};

export const MessagesChat: FunctionComponent<MessagesChatProps> = ({
  textMessages,
  onMessageSubmit,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const messageClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-history-hover',
    [Theme.Light]: 'bg-grey1',
  });
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
      <Gap sizeRem={0.75} />
      <div className='videochat-transcripts px-0.75' ref={videochatTranscriptsRef}>
        {textMessages.map(transcript => (
          <Fragment key={transcript.frontendId}>
            <div
              className={`${messageClassName} flex flex-col py-0.25 px-0.5 rounded-0.5`}
            >
              <Typography size='s' bold>{transcript.userNickname}</Typography>
              <Gap sizeRem={0.25} />
              <Typography size='s'>{transcript.value}</Typography>
              <div className='text-right'>
                <Typography size='xs' secondary>{formatTime(new Date(transcript.createdAt))}</Typography>
              </div>
            </div>
            <Gap sizeRem={0.25} />
          </Fragment>
        ))}
      </div>
      <Gap sizeRem={0.5} />
      <div className='flex justify-between px-0.75'>
        <input
          type='text'
          placeholder={localizationCaptions[LocalizationKey.ChatMessagePlaceholder]}
          ref={messageInputRef}
          className='flex-1'
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
