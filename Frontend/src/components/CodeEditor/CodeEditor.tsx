import React, {
  ChangeEventHandler,
  FunctionComponent,
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
import { Modal } from '../Modal/Modal';
import { Gap } from '../Gap/Gap';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Button } from '../Button/Button';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import {
  ExecuteCodeResult,
  executeCodeWithExpect,
} from '../../utils/executeCodeWithExpect';
import { ModalFooter } from '../ModalFooter/ModalFooter';
import { CodeExecutionResult } from '../CodeExecutionResult/CodeExecutionResult';

import './CodeEditor.css';

export const defaultCodeEditorFontSize = 13;

const fontSizeOptions = [10, 12, 13, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48];

const languagesForExecute: CodeEditorLang[] = [CodeEditorLang.Javascript];

const fontSizeLocalStorageKey = 'codeEditorFontSize';

const readFontSizeFromStorage = () => {
  const parsedValue = Number(localStorage.getItem(fontSizeLocalStorageKey));
  if (isNaN(parsedValue) || typeof parsedValue !== 'number') {
    return defaultCodeEditorFontSize;
  }
  return parsedValue;
};

const saveFontSizeToStorage = (fontSize: number) =>
  localStorage.setItem(fontSizeLocalStorageKey, String(fontSize));

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
  onExecutionResultsSubmit?: (
    code: string | undefined,
    language: CodeEditorLang,
  ) => void;
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
  onExecutionResultsSubmit,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const { themeInUi } = useContext(ThemeContext);
  const [fontSize, setFontSize] = useState(readFontSizeFromStorage());
  const [expectResult, setExpectResult] = useState<ExecuteCodeResult>({
    results: [],
  });
  const expectResultsPassed =
    expectResult.results.length > 0 &&
    expectResult.results.every((expectResult) => expectResult.passed);
  const [modalExpectResults, setModalExpectResults] = useState(false);
  const codeEditorComponentRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    saveFontSizeToStorage(fontSize);
  }, [fontSize]);

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

  const handleExecuteCode = () => {
    const executeCodeResult = executeCodeWithExpect(value);
    setExpectResult(executeCodeResult);
    setModalExpectResults(true);
  };

  const handleExecutionResultsSubmit = () => {
    if (!onExecutionResultsSubmit) {
      return;
    }
    setModalExpectResults(false);
    onExecutionResultsSubmit(value, language);
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
      className={`code-editor flex flex-col rounded-[1.125rem] overflow-hidden ${className}`}
      ref={codeEditorComponentRef}
    >
      <div className="code-editor-tools flex">
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
          onChange={handleFontSizeChange}
        >
          {renderOptions(fontSizeOptions)}
        </select>
        {languagesForExecute.includes(language) && (
          <Button
            className="min-h-[1.75rem] !p-0 text-[0.75rem]"
            onClick={handleExecuteCode}
            disabled={!value?.length}
          >
            {localizationCaptions[LocalizationKey.Run]}
            <Gap sizeRem={0.25} horizontal />
            <Icon inheritFontSize name={IconNames.PaperPlane} />
          </Button>
        )}
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

      <Modal
        contentLabel={
          localizationCaptions[LocalizationKey.ExpectsExecuteResults]
        }
        onClose={handleModalExpectClose}
        open={modalExpectResults}
      >
        <CodeExecutionResult expectResult={expectResult} />
        <Gap sizeRem={1.5} />
        {onExecutionResultsSubmit &&
          expectResultsPassed &&
          !expectResult.error && (
            <ModalFooter>
              <Button onClick={handleModalExpectClose}>
                {localizationCaptions[LocalizationKey.Close]}
              </Button>
              <Button variant="active" onClick={handleExecutionResultsSubmit}>
                {localizationCaptions[LocalizationKey.ExecutionResultsSubmit]}
              </Button>
            </ModalFooter>
          )}
      </Modal>
    </div>
  );
};
