import { Tag } from './tag';

export enum QuestionType {
  Public = 'Public',
  Private = 'Private',
}

export enum CodeEditorLang {
  Plaintext = 'plaintext',
  Markdown = 'markdown',
  C = 'c',
  Cpp = 'cpp',
  Csharp = 'csharp',
  Css = 'css',
  Go = 'go',
  Html = 'html',
  Java = 'java',
  Javascript = 'javascript',
  Kotlin = 'kotlin',
  Mysql = 'mysql',
  Php = 'php',
  Python = 'python',
  Ruby = 'ruby',
  Rust = 'rust',
  Sql = 'sql',
  Swift = 'swift',
  Typescript = 'typescript',
  Xml = 'xml',
  Yaml = 'yaml',
}

export interface QuestionAnswer {
  id: string;
  title: string;
  content: string;
  codeEditor: boolean;
}

export interface Question {
  id: string;
  value: string;
  tags: Tag[];
  codeEditor?: {
    content: string;
    lang: CodeEditorLang;
  } | null;
  answers: QuestionAnswer[];
  category: {
    id: string;
    name: string;
    parentId: string;
  };
}
