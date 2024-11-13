import { FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import { OnChange, OnMount } from '@monaco-editor/react';
import { RemoteCursorManager, RemoteSelectionManager } from '@convergencelabs/monaco-collab-ext';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { SendEventBody, roomsApiDeclaration } from '../../../../apiDeclarations';
import { EventName } from '../../../../constants';
import { AuthContext } from '../../../../context/AuthContext';
import { RemoteCursor } from '@convergencelabs/monaco-collab-ext/typings/RemoteCursor';
import { RemoteSelection } from '@convergencelabs/monaco-collab-ext/typings/RemoteSelection';
import { CodeEditorLang } from '../../../../types/question';
import { CodeEditor, defaultCodeEditorFontSize } from '../../../../components/CodeEditor/CodeEditor';
import { RoomContext } from '../../context/RoomContext';

import './RoomCodeEditor.css';

interface CursorPosition {
  lineNumber: number;
  column: number;
}

const defaultLanguage = CodeEditorLang.Plaintext;

const sendCursorEventTimeout = 22;

const remoteCursorClassName = 'room-code-editor-cursor';
const remoteTooltipClassName = 'room-code-editor-tooltip';
const remoteCursorColor = 'var(--active)';
const remoteSelectionColor = 'var(--active)';

interface RoomCodeEditorProps {
  initialValue: string | null;
}

export const RoomCodeEditor: FunctionComponent<RoomCodeEditorProps> = ({
  initialValue,
}) => {
  const auth = useContext(AuthContext);
  const {
    viewerMode,
    roomState,
    lastWsMessageParsed,
    codeEditorLanguage,
    sendWsMessage,
  } = useContext(RoomContext);
  const [value, setValue] = useState<string | null>(initialValue);
  const [remoteCursor, setRemoteCursor] = useState<RemoteCursor | null>(null);
  const [remoteSelection, setRemoteSelection] = useState<RemoteSelection | null>(null);
  const [cursorPosition, setCursorPosition] = useState<CursorPosition | null>(null);
  const [selectionPosition, setSelectionPosition] = useState<{ start: CursorPosition; end: CursorPosition; } | null>(null);

  const {
    fetchData: sendRoomEvent,
  } = useApiMethod<unknown, SendEventBody>(roomsApiDeclaration.sendEvent);

  useEffect(() => {
    if (typeof initialValue !== 'string') {
      return;
    }
    setValue(initialValue);
  }, [initialValue]);

  useEffect(() => {
    if (!lastWsMessageParsed) {
      return;
    }
    try {
      switch (lastWsMessageParsed?.Type) {
        case 'ChangeCodeEditor':
          if (
            lastWsMessageParsed.Value.Source === 'User' &&
            lastWsMessageParsed.CreatedById === auth?.id
          ) {
            break;
          }
          setValue(lastWsMessageParsed.Value.Content);
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse editor message error: ', err);
    }
  }, [lastWsMessageParsed, auth]);

  const dirtyChangeRemoteCursorHeight = (height: number) => {
    const el = document.querySelector(`.${remoteCursorClassName}`) as HTMLElement;
    if (!el) {
      console.warn('Remote cursor element not found');
      return;
    }
    el.style.height = `${height}px`;
  };

  const handleFontSizeChange = (size: number) => {
    dirtyChangeRemoteCursorHeight(~~(size + (size / 4)));
  };

  const dirtyChangeRemoteCursorTooltip = useCallback((content: string) => {
    const el = document.querySelector(`.${remoteTooltipClassName}`) as HTMLElement;
    if (!el) {
      console.warn('Remote tooltip element not found');
      return;
    }
    el.textContent = content;
  }, []);

  useEffect(() => {
    if (!lastWsMessageParsed || !auth || !remoteCursor || !remoteSelection) {
      return;
    }
    try {
      switch (lastWsMessageParsed.Type) {
        case EventName.CodeEditorCursor:
          if (auth.id === lastWsMessageParsed.Value.UserId) {
            remoteSelection.hide();
            remoteCursor.hide();
            return;
          }
          if (lastWsMessageParsed.Value.AdditionalData.cursor.column === -1) {
            remoteCursor.hide();
            remoteSelection.hide();
            return;
          }
          dirtyChangeRemoteCursorTooltip(lastWsMessageParsed.Value.AdditionalData.nickname);
          remoteCursor.setPosition(lastWsMessageParsed.Value.AdditionalData.cursor);
          remoteCursor.show();
          remoteSelection.setPositions(lastWsMessageParsed.Value.AdditionalData.selection.start, lastWsMessageParsed.Value.AdditionalData.selection.end);
          remoteSelection.show();
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse editor message error: ', err);
    }
  }, [lastWsMessageParsed, remoteSelection, remoteCursor, auth, dirtyChangeRemoteCursorTooltip]);

  useEffect(() => {
    if (!roomState || viewerMode || !cursorPosition || !selectionPosition) {
      console.warn('Cannot send room event');
      return;
    }
    const sendEventTimeoutId = setTimeout(() => {
      sendWsMessage(JSON.stringify({
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
  }, [cursorPosition, selectionPosition, roomState, viewerMode, auth, sendWsMessage]);

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
    dirtyChangeRemoteCursorHeight(defaultCodeEditorFontSize);
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
    if (viewerMode) {
      return;
    }
    sendWsMessage(JSON.stringify({
      Type: 'code',
      Value: value,
    }));
  };

  const handleLanguageChange = (lang: CodeEditorLang) => {
    if (!roomState) {
      console.warn('roomState not found');
      return;
    }
    sendRoomEvent({
      roomId: roomState.id,
      type: EventName.CodeEditorLanguage,
      additionalData: { value: lang },
    });
  };

  return (
    <CodeEditor
      language={codeEditorLanguage || defaultLanguage}
      languages={Object.values(CodeEditorLang)}
      readOnly={viewerMode}
      value={value || ''}
      onMount={handleEditorMount}
      onChange={handleChange}
      onLanguageChange={handleLanguageChange}
      onFontSizeChange={handleFontSizeChange}
    />
  );
};
