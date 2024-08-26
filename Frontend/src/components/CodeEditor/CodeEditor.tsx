import { ChangeEventHandler, FunctionComponent, useContext, useState } from 'react';
import Editor, { OnChange, OnMount, BeforeMount, Monaco } from '@monaco-editor/react';
import { CodeEditorLang } from '../../types/question';
import { Theme, ThemeContext } from '../../context/ThemeContext';

import './CodeEditor.css';

export const defaultCodeEditorFontSize = 13;

const fontSizeOptions = [10, 12, 13, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48];

const renderOptions = (options: Array<number | string>) =>
  options.map(option => (
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
  const { themeInUi } = useContext(ThemeContext);
  const [fontSize, setFontSize] = useState(defaultCodeEditorFontSize);

  const handleFontSizeChange: ChangeEventHandler<HTMLSelectElement> = (event) => {
    const newFontSize = Number(event.target.value);
    setFontSize(newFontSize);
    onFontSizeChange?.(newFontSize);
  };

  const handleLanguageChange: ChangeEventHandler<HTMLSelectElement> = (event) => {
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
    }
    monaco.editor.defineTheme('my-dark', theme);
  };

  return (
    <div className={`code-editor flex flex-col rounded-1.125 overflow-hidden ${className}`}>
      <div className='code-editor-tools'>
        <select className='code-editor-tools-select' value={language} disabled={!onLanguageChange} onChange={handleLanguageChange}>
          {renderOptions(languages)}
        </select>
        <select className='code-editor-tools-select' value={fontSize} disabled={!onFontSizeChange} onChange={handleFontSizeChange}>
          {renderOptions(fontSizeOptions)}
        </select>
      </div>
      <div className='flex-1'>
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
