import { ChangeEventHandler, FunctionComponent, useContext, useEffect, useRef, useState } from 'react';
import Editor, { OnChange } from '@monaco-editor/react';
import { SendMessage } from 'react-use-websocket';
import { Theme, ThemeContext } from '../../../../context/ThemeContext';
import { RoomState } from '../../../../types/room';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

import './CodeEditor.css';

const languageOptions = [
  'plaintext',
  'c',
  'cpp',
  'csharp',
  'css',
  'go',
  'html',
  'java',
  'javascript',
  'kotlin',
  'mysql',
  'php',
  'python',
  'ruby',
  'rust',
  'sql',
  'swift',
  'typescript',
  'xml',
  'yaml',
];

const fontSizeOptions = [10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48];

const renderOptions = (options: Array<number | string>) =>
  options.map(option => (
    <option key={option} value={option}>
      {option}
    </option>
  ));

interface CodeEditorProps {
  roomState: RoomState | null;
  readOnly: boolean;
  lastWsMessage: MessageEvent<any> | null;
  onSendWsMessage: SendMessage;
}

export const CodeEditor: FunctionComponent<CodeEditorProps> = ({
  roomState,
  readOnly,
  lastWsMessage,
  onSendWsMessage,
}) => {
  const { themeInUi } = useContext(ThemeContext);
  const localizationCaptions = useLocalizationCaptions();
  const ignoreChangeRef = useRef(false);
  const [value, setValue] = useState<string>('');
  const [fontSize, setFontSize] = useState(22);
  const [language, setLanguage] = useState<string>('plaintext');

  useEffect(() => {
    if (!roomState) {
      return;
    }
    setValue(roomState.codeEditorContent || '');
  }, [roomState]);

  useEffect(() => {
    if (!lastWsMessage?.data) {
      return;
    }
    try {
      const parsedData = JSON.parse(lastWsMessage?.data);
      const value = parsedData.Value;
      if (typeof value !== 'string') {
        return;
      }
      switch (parsedData?.Type) {
        case 'ChangeCodeEditor':
          if (ignoreChangeRef.current) {
            ignoreChangeRef.current = false;
            break;
          }
          setValue(value);
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse editor message error: ', err);
    }
  }, [lastWsMessage]);

  const handleChange: OnChange = (value) => {
    if (readOnly) {
      return;
    }
    onSendWsMessage(JSON.stringify({
      Type: 'code',
      Value: value,
    }));
    ignoreChangeRef.current = true;
  };

  const handleFontSizeChange: ChangeEventHandler<HTMLSelectElement> = (event) => {
    setFontSize(Number(event.target.value));
  };

  const handleLanguageChange: ChangeEventHandler<HTMLSelectElement> = (event) => {
    setLanguage(event.target.value);
  };

  return (
    <div className='code-editor'>
      <div className='code-editor-tools'>
        <span>{localizationCaptions[LocalizationKey.Language]}:</span>
        <select className='code-editor-tools-select' value={language} onChange={handleLanguageChange}>
          {renderOptions(languageOptions)}
        </select>
        <span>{localizationCaptions[LocalizationKey.FontSize]}:</span>
        <select className='code-editor-tools-select' value={fontSize} onChange={handleFontSizeChange}>
          {renderOptions(fontSizeOptions)}
        </select>
      </div>
      <Editor
        keepCurrentModel={true}
        options={{
          minimap: { enabled: false },
          fontSize,
          quickSuggestions: false,
          readOnly,
        }}
        language={language}
        theme={themeInUi === Theme.Dark ? 'vs-dark' : 'light'}
        value={value}
        onChange={handleChange}
      />
    </div>
  );
};
