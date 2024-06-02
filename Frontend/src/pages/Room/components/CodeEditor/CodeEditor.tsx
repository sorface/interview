import { ChangeEventHandler, FunctionComponent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import Editor, { OnChange, OnMount } from '@monaco-editor/react';
import { RemoteCursorManager, RemoteSelectionManager } from '@convergencelabs/monaco-collab-ext';
import { SendMessage } from 'react-use-websocket';
import { Theme, ThemeContext } from '../../../../context/ThemeContext';
import { RoomState } from '../../../../types/room';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { SendEventBody, roomsApiDeclaration } from '../../../../apiDeclarations';
import { EventName } from '../../../../constants';
import { AuthContext } from '../../../../context/AuthContext';
import { RemoteCursor } from '@convergencelabs/monaco-collab-ext/typings/RemoteCursor';
import { RemoteSelection } from '@convergencelabs/monaco-collab-ext/typings/RemoteSelection';

import './CodeEditor.css';

interface CursorPosition {
  lineNumber: number;
  column: number;
}

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

const defaultLanguage = languageOptions[0];

const fontSizeOptions = [10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48];

const sendCursorEventTimeout = 22;

const remoteCursorClassName = 'code-editor-cursor';
const remoteTooltipClassName = 'code-editor-tooltip';
const remoteCursorColor = 'var(--active)';
const remoteSelectionColor = 'var(--active)';

const renderOptions = (options: Array<number | string>) =>
  options.map(option => (
    <option key={option} value={option}>
      {option}
    </option>
  ));

interface CodeEditorProps {
  language: string;
  roomState: RoomState | null;
  readOnly: boolean;
  lastWsMessage: MessageEvent<any> | null;
  onSendWsMessage: SendMessage;
}

export const CodeEditor: FunctionComponent<CodeEditorProps> = ({
  language,
  roomState,
  readOnly,
  lastWsMessage,
  onSendWsMessage,
}) => {
  const auth = useContext(AuthContext);
  const { themeInUi } = useContext(ThemeContext);
  const localizationCaptions = useLocalizationCaptions();
  const ignoreChangeRef = useRef(false);
  const [value, setValue] = useState<string>('');
  const [fontSize, setFontSize] = useState(22);
  const [remoteCursor, setRemoteCursor] = useState<RemoteCursor | null>(null);
  const [remoteSelection, setRemoteSelection] = useState<RemoteSelection | null>(null);
  const [cursorPosition, setCursorPosition] = useState<CursorPosition | null>(null);
  const [selectionPosition, setSelectionPosition] = useState<{ start: CursorPosition; end: CursorPosition; } | null>(null);

  const {
    fetchData: sendRoomEvent,
  } = useApiMethod<unknown, SendEventBody>(roomsApiDeclaration.sendEvent);

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

  const dirtyChangeRemoteCursorHeight = (height: number) => {
    const el = document.querySelector(`.${remoteCursorClassName}`) as HTMLElement;
    if (!el) {
      console.warn('Remote cursor element not found');
      return;
    }
    el.style.height = `${height}px`;
  };

  useEffect(() => {
    dirtyChangeRemoteCursorHeight(~~(fontSize + (fontSize / 4)));
  }, [fontSize]);

  const dirtyChangeRemoteCursorTooltip = useCallback((content: string) => {
    const el = document.querySelector(`.${remoteTooltipClassName}`) as HTMLElement;
    if (!el) {
      console.warn('Remote tooltip element not found');
      return;
    }
    el.textContent = content;
  }, []);

  useEffect(() => {
    if (!lastWsMessage?.data || !auth || !remoteCursor || !remoteSelection) {
      return;
    }
    try {
      const parsedData = JSON.parse(lastWsMessage?.data);
      const value = parsedData.Value;
      switch (parsedData?.Type) {
        case EventName.CodeEditorCursor:
          if (auth.id === value.UserId) {
            remoteSelection.hide();
            remoteCursor.hide();
            return;
          }
          if (value.AdditionalData.cursor.column === -1) {
            remoteCursor.hide();
            remoteSelection.hide();
            return;
          }
          dirtyChangeRemoteCursorTooltip(value.AdditionalData.nickname);
          remoteCursor.setPosition(value.AdditionalData.cursor);
          remoteCursor.show();
          remoteSelection.setPositions(value.AdditionalData.selection.start, value.AdditionalData.selection.end);
          remoteSelection.show();
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse editor message error: ', err);
    }
  }, [lastWsMessage, remoteSelection, remoteCursor, auth, dirtyChangeRemoteCursorTooltip]);

  useEffect(() => {
    if (!roomState || readOnly || !cursorPosition || !selectionPosition) {
      console.warn('Cannot send room event');
      return;
    }
    const sendEventTimeoutId = setTimeout(() => {
      onSendWsMessage(JSON.stringify({
        Type: EventName.CodeEditorCursor,
        Value: JSON.stringify({
          cursor: cursorPosition,
          selection: selectionPosition,
          id: auth?.id,
          nickname: auth?.nickname,
        }),
      }));
    }, sendCursorEventTimeout);
    return () => {
      clearTimeout(sendEventTimeoutId);
    };
  }, [cursorPosition, selectionPosition, roomState, readOnly, auth, onSendWsMessage]);

  const handleEditorMount: OnMount = (mountedEditor) => {
    const newRemoteCursorManager = new RemoteCursorManager({
      editor: mountedEditor,
      tooltips: true,
      className: remoteCursorClassName,
      showTooltipOnHover: true,
      tooltipDuration: 2,
      tooltipClassName: remoteTooltipClassName,
    });
    const cursor = newRemoteCursorManager.addCursor('cursor1', remoteCursorColor, '');
    cursor.hide();
    setRemoteCursor(cursor);
    dirtyChangeRemoteCursorHeight(fontSize);
    const remoteSelectionManager = new RemoteSelectionManager({ editor: mountedEditor });
    const selection = remoteSelectionManager.addSelection('selection1', remoteSelectionColor);
    setRemoteSelection(selection);

    mountedEditor.onDidChangeCursorPosition(e => {
      setCursorPosition(e.position);
    });
    mountedEditor.onDidChangeCursorSelection(e => {
      setSelectionPosition({
        start: { column: e.selection.startColumn, lineNumber: e.selection.startLineNumber },
        end: { column: e.selection.endColumn, lineNumber: e.selection.endLineNumber },
      })
    });
    mountedEditor.onDidBlurEditorWidget(() => {
      setCursorPosition({ column: -1, lineNumber: -1 });
    });
  };

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
    if (!roomState) {
      console.warn('roomState not found');
      return;
    }
    sendRoomEvent({
      roomId: roomState.id,
      type: EventName.CodeEditorLanguage,
      additionalData: { value: event.target.value },
    });
  };

  return (
    <div className='code-editor'>
      <div className='code-editor-tools'>
        <span>{localizationCaptions[LocalizationKey.Language]}:</span>
        <select className='code-editor-tools-select' value={language || defaultLanguage} onChange={handleLanguageChange}>
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
        language={language || defaultLanguage}
        theme={themeInUi === Theme.Dark ? 'vs-dark' : 'light'}
        value={value}
        onChange={handleChange}
        onMount={handleEditorMount}
      />
    </div>
  );
};
