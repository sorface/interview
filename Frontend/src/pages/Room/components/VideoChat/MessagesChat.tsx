import { FunctionComponent, useRef, KeyboardEvent, useEffect, useState, useContext } from 'react';
import { Transcript } from '../../../../types/transcript';
import { stringToColor } from './utils/stringToColor';
import { Tab, Tabs } from '../../../../components/Tabs/Tabs';
import { ThemeContext } from '../../../../context/ThemeContext';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

import './MessagesChat.css';

interface MessagesChatProps {
  transcripts: Transcript[];
  onMessageSubmit: (message: string) => void;
}

export const MessagesChat: FunctionComponent<MessagesChatProps> = ({
  transcripts,
  onMessageSubmit,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const chatTab: Tab = {
    id: 'chat-tab',
    caption: localizationCaptions[LocalizationKey.ChatTab],
  };
  
  const recognitionTab: Tab = {
    id: 'recognition-tab',
    caption: localizationCaptions[LocalizationKey.RecognitionTab],
  };
  
  const tabs = [
    chatTab,
    recognitionTab,
  ];

  const { themeInUi } = useContext(ThemeContext);
  const messageInputRef = useRef<HTMLInputElement>(null);
  const videochatTranscriptsRef = useRef<HTMLDivElement>(null);
  const [activeTabId, setActiveTabId] = useState(chatTab.id);
  const transcriptsFiltered = transcripts.filter(transcript =>
    activeTabId === chatTab.id ? transcript.fromChat : !transcript.fromChat
  );

  useEffect(() => {
    const chatEl = videochatTranscriptsRef.current;
    if (!chatEl) {
      return;
    }
    const scrollHeight = chatEl.scrollHeight;
    const height = chatEl.clientHeight;
    const maxScrollTop = scrollHeight - height;
    chatEl.scrollTop = maxScrollTop > 0 ? maxScrollTop : 0;
  }, [transcriptsFiltered]);

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
      <Tabs
        tabs={tabs}
        activeTabId={activeTabId}
        onTabClick={setActiveTabId}
      />
      <div className='videochat-transcripts' ref={videochatTranscriptsRef}>
        {transcriptsFiltered.map(transcript => (
          <div
            key={transcript.frontendId}
          >
            <span>
              {!transcript.fromChat && `${localizationCaptions[LocalizationKey.Recognized]} `}
            </span>
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
      {activeTabId === chatTab.id && (
        <div className='message-input-box'>
          <div className='message-input-wrapper'>
            <input
              type='text'
              placeholder={localizationCaptions[LocalizationKey.ChatMessagePlaceholder]}
              ref={messageInputRef}
              onKeyDown={handleInputKeyDown}
            />
          </div>
          <div>
            <button onClick={handleChatMessageSubmit}>{localizationCaptions[LocalizationKey.SendToChat]}</button>
          </div>
        </div>
      )}
    </div>
  );
};
