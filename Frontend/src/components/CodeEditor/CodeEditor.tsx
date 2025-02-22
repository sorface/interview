import React, {
  ChangeEventHandler,
  FunctionComponent,
  ReactNode,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
import Editor, {
  OnChange,
  OnMount,
  BeforeMount,
  Monaco,
} from '@monaco-editor/react';
import { CodeEditorLang } from '../../types/question';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import {
  Res,
  RunCodeButton,
} from '../../pages/Room/components/RunCodeButton/RunCodeButton';

import './CodeEditor.css';
import { Modal } from '../Modal/Modal';
import { Gap } from '../Gap/Gap';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

export const defaultCodeEditorFontSize = 13;

const fontSizeOptions = [10, 12, 13, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48];

const renderOptions = (options: Array<number | string>) =>
  options.map((option) => (
    <option key={option} value={option}>
      {option}
    </option>
  ));

interface CodeEditorProps {
  language: CodeEditorLang;
  languages: CodeEditorLang[];
  className?: string;
  readOnly?: boolean;
  value?: string | undefined;
  scrollBeyondLastLine?: boolean;
  alwaysConsumeMouseWheel?: boolean;
  onMount?: OnMount | undefined;
  onChange?: OnChange | undefined;
  onLanguageChange?: (language: CodeEditorLang) => void;
  onFontSizeChange?: (size: number) => void;
}

export const CodeEditor: FunctionComponent<CodeEditorProps> = ({
  language,
  languages,
  className,
  readOnly,
  value,
  scrollBeyondLastLine,
  alwaysConsumeMouseWheel,
  onMount,
  onChange,
  onLanguageChange,
  onFontSizeChange,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const { themeInUi } = useContext(ThemeContext);
  const [fontSize, setFontSize] = useState(defaultCodeEditorFontSize);
  const [expectResults, setExpectResults] = useState<Res>({});
  const [modalExpectResults, setModalExpectResults] = useState(false);
  const codeEditorComponentRef = useRef<HTMLDivElement | null>(null);

  const handleExpectResults = (arr: Res) => {
    setExpectResults(arr);
    setModalExpectResults(true);
  };
  const handleModalExpectClose = () => setModalExpectResults(false);

  const handleFontSizeChange: ChangeEventHandler<HTMLSelectElement> = (
    event,
  ) => {
    const newFontSize = Number(event.target.value);
    setFontSize(newFontSize);
    onFontSizeChange?.(newFontSize);
  };

  const handleLanguageChange: ChangeEventHandler<HTMLSelectElement> = (
    event,
  ) => {
    onLanguageChange?.(event.target.value as CodeEditorLang);
  };

  const handleBeforeMount: BeforeMount = (monaco: Monaco) => {
    const theme = {
      base: 'vs-dark' as const,
      inherit: true,
      rules: [],
      colors: {
        'editor.background': '#233149',
        'editor.lineHighlightBackground': '#FFFFFF0F',
      },
    };
    monaco.editor.defineTheme('my-dark', theme);
  };

  useEffect(() => {
    const handleCodeEditorKeyDown = (e: KeyboardEvent) => {
      if (
        e.key === 's' &&
        (navigator.userAgent.includes('Mac') ? e.metaKey : e.ctrlKey)
      ) {
        e.preventDefault();
      }
    };

    const codeEditorComponent = codeEditorComponentRef.current;
    codeEditorComponent?.addEventListener(
      'keydown',
      handleCodeEditorKeyDown,
      false,
    );

    return () => {
      codeEditorComponent?.removeEventListener(
        'keydown',
        handleCodeEditorKeyDown,
        false,
      );
    };
  }, []);

  return (
    <div
      className={`code-editor flex flex-col rounded-1.125 overflow-hidden ${className}`}
      ref={codeEditorComponentRef}
    >
      <Modal
        contentLabel="Modal with expect results"
        onClose={handleModalExpectClose}
        open={modalExpectResults}
      >
        {localizationCaptions[LocalizationKey.CodeEditorResults]}
        <br />

        {Object.entries(expectResults)?.map(([key, expectCall]) => {
          const firstValue = JSON.stringify(
            expectCall?.arguments?.[0]?.[0],
          ) as ReactNode;
          const secondValue = JSON.stringify(
            expectCall?.arguments?.[0]?.[1],
          ) as ReactNode;
          const expectResult = JSON.stringify(expectCall.result);

          const resultValue = `expect(${firstValue},${secondValue}) : ${expectResult}`;

          return <div key={key}>{resultValue}</div>;
        })}
        <Gap sizeRem={1} />
      </Modal>
      <div className="code-editor-tools">
        <select
          className="code-editor-tools-select"
          value={language}
          disabled={!onLanguageChange}
          onChange={handleLanguageChange}
        >
          {renderOptions(languages)}
        </select>
        <select
          className="code-editor-tools-select"
          value={fontSize}
          disabled={!onFontSizeChange}
          onChange={handleFontSizeChange}
        >
          {renderOptions(fontSizeOptions)}
        </select>
        <RunCodeButton
          handleExpectResults={handleExpectResults}
          codeEditorText={value}
        />
      </div>
      <div className="flex-1">
        <Editor
          keepCurrentModel={true}
          options={{
            minimap: { enabled: false },
            fontSize,
            quickSuggestions: false,
            readOnly,
            scrollBeyondLastLine,
            scrollbar: {
              alwaysConsumeMouseWheel,
            },
          }}
          language={language}
          theme={themeInUi === Theme.Dark ? 'my-dark' : 'light'}
          value={value}
          onChange={onChange}
          onMount={onMount}
          beforeMount={handleBeforeMount}
        />
      </div>
    </div>
  );
};
